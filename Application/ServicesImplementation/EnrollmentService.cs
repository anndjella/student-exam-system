using Application.Common;
using Application.DTO.Common;
using Application.DTO.Enrollments;
using Application.DTO.Me.Student;
using Application.DTO.Subjects;
using Application.DTO.Teachers;
using Application.Services;
using Domain.Entity;
using Domain.Enums;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ServicesImplementation
{
    public sealed class EnrollmentService : IEnrollmentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IClock _clock;

        public EnrollmentService(IUnitOfWork uow, IClock clock)
        {
            _uow= uow;
            _clock= clock;
        }

        public async Task<BulkEnrollResult> BulkEnrollByIndexYearAsync(
            BulkEnrollByIndexYearRequest req,
            CancellationToken ct)
        {                    
            var distinctSubjectIds = req.SubjectIds.Distinct().ToList();

            var existingSubjects = await _uow.Subjects.GetExistingIdsAsync(distinctSubjectIds, ct);
            if (existingSubjects.Count != distinctSubjectIds.Count)
            {
                var missing = distinctSubjectIds.Except(existingSubjects).ToList();
                throw new AppException(AppErrorCode.NotFound, $"Some subjects not found: {string.Join(",", missing)}");
            }

            var prefix = req.IndexStartYear.ToString(CultureInfo.InvariantCulture) + "/";
            var studentIds = await _uow.Students.ListIdsByIndexPrefixAsync(prefix, ct);

            if (studentIds.Count == 0)
                return new BulkEnrollResult(StudentsMatched: 0, EnrollmentsCreated: 0, EnrollmentsSkipped: 0);

            var existingPairs = await _uow.Enrollments.ListExistingPairsAsync(
                studentIds,
                distinctSubjectIds,
                ct);

            var toCreate = new List<Enrollment>(capacity: studentIds.Count * distinctSubjectIds.Count);

            foreach (var sid in studentIds)
            {
                foreach (var subId in distinctSubjectIds)
                {
                    if (existingPairs.Contains((sid, subId)))
                        continue;

                    toCreate.Add(new Enrollment
                    {
                        StudentID = sid,
                        SubjectID = subId,
                        CreatedAt=DateTime.UtcNow,
                        IsPassed = false
                    });
                }
            }

            _uow.Enrollments.AddRange(toCreate);
            await _uow.CommitAsync(ct);

            var totalRequested = studentIds.Count * distinctSubjectIds.Count;
            var created = toCreate.Count;
            var skipped = totalRequested - created;

            return new BulkEnrollResult(
                StudentsMatched: studentIds.Count,
                EnrollmentsCreated: created,
                EnrollmentsSkipped: skipped
            );
        }
        // for 'all subjects'
        public async Task<List<SubjectResponse>> ListStudentSubjectsAsync(int studentId, CancellationToken ct = default)
        {
            if (await _uow.Students.GetByIdAsync(studentId, ct) is null)
                throw new AppException(AppErrorCode.NotFound, $"Student with id {studentId} not found.");

            var enrollments = await _uow.Enrollments.ListByStudentIdWithSubjectAndTeachersAsync(studentId, ct);
            return enrollments
                .Where(e=>e.Subject != null)
                .Select(e => Mapper.SubjectToResponse(e.Subject!))
                .ToList();
        }
        // for 'subjects I can register'
        public async Task<List<SubjectResponse>> ListNotPassedAsync(int studentId, CancellationToken ct)
        {
            if (await _uow.Students.GetByIdAsync(studentId, ct) is null)
                throw new AppException(AppErrorCode.NotFound, $"Student with id {studentId} not found.");
            var enrollments=await _uow.Enrollments.ListNotPassed(studentId, ct);    
            return enrollments.Where(e => e.Subject != null).Select(e=>Mapper.SubjectToResponse(e.Subject)).ToList();  
        }

        //public async Task<PagedResponse<EnrollmentResponse>> ListAsync(int skip, int take, string? query, CancellationToken ct)
        //{
        //    if (skip < 0) skip = 0;
        //    if (take <= 0) take = 20;
        //    if (take > 100) take = 100;

        //    var total = await _uow.Enrollments.CountAsync(query, ct);
        //    var items = await _uow.Enrollments.ListPagedAsync(skip, take, query, ct);

        //    var respItems = items.Select(Mapper.EnrollmentToResponse).ToList();

        //    return new PagedResponse<EnrollmentResponse>
        //    {
        //        Items = respItems,
        //        Total = total
        //    };
        //}

        public async Task DeleteAsync(int studentId, int subjectId, CancellationToken ct = default)
        {
            var enrollment = await _uow.Enrollments.GetAsync(studentId, subjectId, ct) ??
                throw new AppException(AppErrorCode.NotFound, $"Enrollment with subject {subjectId} and student {studentId} not found.");

            if(await _uow.Registrations.ExistsAnyForSubjectAndStudentAsync(subjectId,studentId,ct))
                throw new AppException(AppErrorCode.Conflict, "Enrollment cannot be deleted because it has registrations.");

            if (await _uow.Exams.ExistsAnyForSubjectAndStudentAsync(subjectId, studentId, ct))
                throw new AppException(AppErrorCode.Conflict, "Enrollment cannot be deleted because it has exams.");

            _uow.Enrollments.Remove(enrollment);
           await _uow.CommitAsync(ct);
        }

        public async Task<EnrollmentResponse> CreateAsync(CreateEnrollmentRequest req,CancellationToken ct = default)
        {
            var student = await _uow.Students.GetByIndexAsync(req.StudentIndex, ct);
            if (student is null)
                throw new AppException(AppErrorCode.NotFound, $"Student with index {req.StudentIndex} not found.");

            var subject = await _uow.Subjects.GetByCodeAsync(req.SubjectCode, ct);
            if (subject is null)
                throw new AppException(AppErrorCode.NotFound, $"Subject with code {req.SubjectCode} not found.");

            var exists = await _uow.Enrollments.ExistsAsync(student.ID, subject.ID, ct);
            if (exists)
                throw new AppException(AppErrorCode.Conflict, "Enrollment already exists.");

            var e = new Enrollment
            {
                StudentID = student.ID,
                SubjectID = subject.ID,
                CreatedAt= _clock.UtcNow
            };

             _uow.Enrollments.Add(e);
            await _uow.CommitAsync(ct);

            var created = await _uow.Enrollments.GetAsync(student.ID,subject.ID,ct);
            if (created is null)
                throw new AppException(AppErrorCode.Unexpected, "Unexpected error in creating.");

            return Mapper.EnrollmentToResponse(created);
        }

        public async Task<PagedResponse<EnrollmentResponse>> ListByStudentAsync(string index, int skip, int take, string? query, CancellationToken ct)
        {
            if (skip < 0) skip = 0;
            if (take <= 0) take = 20;
            if (take > 100) take = 100;

            index = Uri.UnescapeDataString(index ?? "").Trim();

            var student = await _uow.Students.GetByIndexAsync(index, ct);
            if (student is null)
                throw new AppException(AppErrorCode.NotFound, $"Student with index {index} not found.");

            var total = await _uow.Enrollments.CountByStudentAsync(student.ID, query, ct);              
            var items = await _uow.Enrollments.ListPagedByStudentAsync(student.ID,skip, take, query, ct);

            var respItems = items.Select(Mapper.EnrollmentToResponse).ToList();

            return new PagedResponse<EnrollmentResponse>
            {
                Items = respItems,
                Total = total
            };
        }

        public async Task<PagedResponse<EnrollmentResponse>> ListBySubjectAsync(string code, int skip, int take, string? query, CancellationToken ct)
        {
            if (skip < 0) skip = 0;
            if (take <= 0) take = 20;
            if (take > 100) take = 100;

            var subject = await _uow.Subjects.GetByCodeAsync(code, ct);
            if (subject is null)
                throw new AppException(AppErrorCode.NotFound, $"Subject with code {code} not found.");

            var total = await _uow.Enrollments.CountBySubjectAsync(subject.ID, query, ct);
            var items = await _uow.Enrollments.ListPagedBySubjectAsync(subject.ID, skip, take, query, ct);

            var respItems = items.Select(Mapper.EnrollmentToResponse).ToList();

            return new PagedResponse<EnrollmentResponse>
            {
                Items = respItems,
                Total = total
            };
        }
    } 
}
