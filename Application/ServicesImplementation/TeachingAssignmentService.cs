using Application.Common;
using Application.DTO.Me.Student;
using Application.DTO.Me.Teacher;
using Application.DTO.Subjects;
using Application.DTO.TeachingAssignment;
using Application.Services;
using Domain.Entity;
using Domain.Enums;
using Microsoft.AspNetCore.Server.HttpSys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ServicesImplementation
{
    public sealed class TeachingAssignmentService : ITeachingAssignmentService
    {
        private readonly IUnitOfWork _uow;
        public TeachingAssignmentService(IUnitOfWork uow) { _uow = uow; }
        public async Task<bool> CanTeacherGradeAsync(int teacherId, int subjectId, CancellationToken ct)
        {
           var ta= await _uow.TeachingAssignments.GetAsync(teacherId, subjectId, ct);

           if (ta is null)
               throw new AppException(AppErrorCode.NotFound, $"Teaching assignment not found.");

            return ta?.CanGrade ?? false;
        }

        public async Task<TeachingAssignmentResponse> CreateAsync(CreateTeachingAssignmentRequest req, CancellationToken ct)
        {
            if (await _uow.TeachingAssignments.GetAsync(req.TeacherID, req.SubjectID, ct) is not null)
                throw new AppException(AppErrorCode.Validation, "This teaching assignment already exists.");
           
            if (await _uow.Teachers.GetByIdAsync(req.TeacherID, ct) is null)    
                throw new AppException(AppErrorCode.NotFound, $"Teacher with id {req.TeacherID} not found");

            if (await _uow.Subjects.GetByIdWithTeachersAsync(req.SubjectID, ct) is null)
                throw new AppException(AppErrorCode.NotFound, $"Subject with id {req.SubjectID} not found");

            TeachingAssignment ta = new TeachingAssignment
            {
                SubjectID = req.SubjectID,
                TeacherID = req.TeacherID,
                CanGrade = req.CanGrade
            };

            _uow.TeachingAssignments.Add(ta);
            await _uow.CommitAsync(ct);

            var created = await _uow.TeachingAssignments.GetAsync(req.TeacherID, req.SubjectID, ct) ??
                   throw new AppException(AppErrorCode.NotFound, "Unexpected error in creating teaching assignment");
            return Mapper.TeachingAssignmentToResponse(created);
        }

        public async Task DeleteAsync(int teacherId, int subjectId, CancellationToken ct)
        {
           var ta= await _uow.TeachingAssignments.GetAsync(teacherId, subjectId, ct) ??
                throw new AppException(AppErrorCode.NotFound,$"Teaching assignment with teacherId {teacherId} and subjectId {subjectId} not found.");
          
            _uow.TeachingAssignments.Remove(ta);
           
            await _uow.CommitAsync(ct);      
        }

        public async Task<TeachingAssignmentResponse> GetAsync(int teacherId, int subjectId, CancellationToken ct)
        {
            var ta = await _uow.TeachingAssignments.GetAsync(teacherId, subjectId, ct) ??
               throw new AppException(AppErrorCode.NotFound, $"Teaching assignment with teacherId {teacherId} and subjectId {subjectId} not found.");
           
            return Mapper.TeachingAssignmentToResponse(ta);
        }

        public async Task<TeacherSubjectsResponse> ListTeacherSubjectsDividedAsync(int personId, CancellationToken ct)
        {
            var teacher = await _uow.Teachers.GetByIdAsync(personId, ct);
            if (teacher is null)
                throw new AppException(AppErrorCode.NotFound, $"Teacher with id {personId} not found.");
            var teachingAssignments = await _uow.TeachingAssignments.ListByTeacherIdWithSubjectAndTeachersAsync(teacher.ID, ct);

            var dto = new TeacherSubjectsResponse();

            foreach (var ta in teachingAssignments)
            {
                if (ta.Subject is null)
                    continue;

                var mapped = Mapper.SubjectToResponse(ta.Subject);

                if (ta.CanGrade)
                    dto.GradableSubjects.Add(mapped);
                else
                    dto.NonGradableSubjects.Add(mapped);
            }

            return dto;
        }

        public async Task<List<TeachingAssignmentResponse>> ListBySubjectAsync(int subjectId, CancellationToken ct)
        {
            if (await _uow.Subjects.GetByIdWithTeachersAsync(subjectId, ct) is null)
                throw new AppException(AppErrorCode.NotFound, $"Subject with id {subjectId} not found");

            var list = await _uow.TeachingAssignments.ListBySubjectIdAsync(subjectId, ct);

            return list.Select(Mapper.TeachingAssignmentToResponse).ToList();
        }

        public async Task<List<TeachingAssignmentResponse>> ListByTeacherAsync(int teacherId, CancellationToken ct)
        {
            if (await _uow.Teachers.GetByIdAsync(teacherId, ct) is null)
                throw new AppException(AppErrorCode.NotFound, $"Teacher with id {teacherId} not found");
            
            var list= await _uow.TeachingAssignments.ListByTeacherIdAsync(teacherId,ct);

            return list.Select(Mapper.TeachingAssignmentToResponse).ToList();
        }

        public async Task UpdateCanGradeAsync(CreateTeachingAssignmentRequest req, CancellationToken ct)
        {
            var ta = await _uow.TeachingAssignments.GetAsync(req.TeacherID, req.SubjectID, ct) ??
                 throw new AppException(AppErrorCode.NotFound, $"Teaching assignment with teacherId {req.TeacherID} and subjectId {req.SubjectID} not found.");
          
            ta.CanGrade = req.CanGrade;
          
            await _uow.CommitAsync();

        }
        //public async Task UpdateSubjectTeachersAsync(int subjectId, UpdateSubjectTeachersRequest req, CancellationToken ct)
        //{
        //    var subject = await _uow.Subjects.GetByIdAsync(subjectId, ct);
        //    if (subject == null)
        //        throw new AppException(AppErrorCode.NotFound, $"Subject with id {subjectId} not found.");

        //    var currentAssignments = await _uow.TeachingAssignments.ListBySubjectIdAsync(subjectId, ct);

        //    foreach (var assignment in currentAssignments)
        //    {
        //        _uow.TeachingAssignments.Remove(assignment);
        //    }

        //    foreach (var tInfo in req.Teachers)
        //    {
        //        if (!await _uow.Teachers.ExistsByIdAsync(tInfo.TeacherID, ct))               
        //            throw new AppException(AppErrorCode.NotFound, $"Teacher with id {tInfo.TeacherID} not found.");

        //        var newAssignment = new TeachingAssignment
        //        {
        //            SubjectID = subjectId,
        //            TeacherID = tInfo.TeacherID,
        //            CanGrade = tInfo.CanGrade
        //        };
        //        _uow.TeachingAssignments.Add(newAssignment);
        //    }
        //    await _uow.CommitAsync(ct);
        //}
    }
}
