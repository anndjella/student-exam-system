using Application.Common;
using Application.DTO.Common;
using Application.DTO.Registrations;
using Application.Services;
using Domain.Entity;
using Domain.Enums;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ServicesImplementation
{
    public sealed class RegistrationService : IRegistrationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IClock _clock;
        public RegistrationService(IUnitOfWork uow,IClock clock)
        {
            _uow = uow;
            _clock = clock;
        }

        private async Task ValidateRegistrationPreconditionsAsync(int studentId, int subjectId, int termId, CancellationToken ct)
        {
            var term = await _uow.Terms.GetByIdAsync(termId, ct)
                ?? throw new AppException(AppErrorCode.NotFound, $"Term {termId} not found.");

            if (!term.IsInRegistrationWindow(_clock.Today))
                throw new AppException(AppErrorCode.Conflict, "Registration window is closed.");

            if (!await _uow.Enrollments.ExistsAsync(studentId, subjectId, ct))
                throw new AppException(AppErrorCode.Validation, "Student is not enrolled in this subject");

            if (await _uow.Enrollments.IsPassedAsync(studentId, subjectId, ct))
                throw new AppException(AppErrorCode.Validation, "Student already passed this subject");
        }

        public async Task<StudentRegistrationResponse> CreateAsync(int studentId, CreateRegistrationRequest req, CancellationToken ct = default)
        {
            await ValidateRegistrationPreconditionsAsync(studentId,req.SubjectID, req.TermID, ct);

            var existing = await _uow.Registrations.GetAsync(studentId, req.SubjectID, req.TermID, ct);

            if (existing is null)
            {
                var r = new Registration
                {
                    StudentID = studentId,
                    SubjectID = req.SubjectID,
                    TermID = req.TermID,
                    IsActive = true,
                    RegisteredAt = _clock.UtcNow,
                    CancelledAt = null
                };

                _uow.Registrations.Add(r);
                await _uow.CommitAsync(ct);

                return Mapper.StudentRegistrationToResponse(r);
            }

            if (existing.IsActive)
                throw new AppException(AppErrorCode.Conflict, "Subject is already registered for this term.");

            existing.IsActive = true;
            existing.RegisteredAt = _clock.UtcNow;
            existing.CancelledAt = null;

            await _uow.CommitAsync(ct);
            return Mapper.StudentRegistrationToResponse(existing);
        }

        public async Task CancelAsync(int studentId, int subjectId, int termId, CancellationToken ct = default)
        {
            await ValidateRegistrationPreconditionsAsync(studentId, subjectId, termId, ct);

            var existing = await _uow.Registrations.GetAsync(studentId, subjectId, termId, ct)
                ?? throw new AppException(AppErrorCode.NotFound, "Registration not found.");

            if (!existing.IsActive)
                throw new AppException(AppErrorCode.Conflict, "Registration is not active.");

            existing.IsActive = false;
            existing.CancelledAt = _clock.UtcNow;

            await _uow.CommitAsync(ct);
        }

        public async Task<List<StudentRegistrationResponse>> ListMyActiveAsync(int studentId, CancellationToken ct = default)
        {
            var list = await _uow.Registrations.ListActiveForStudentWithExamAsync(studentId, ct);
            return list.Select(Mapper.StudentRegistrationToResponse).ToList();
        }

        public async Task<List<TeacherRegistrationResponse>> ListMyActiveBySubjectAndTermAsync(int teacherId,int subjectId, int termId, CancellationToken ct = default)
        {
            var exists = await _uow.TeachingAssignments.ExistsAsync(teacherId, subjectId, ct);
            if (!exists)
                throw new AppException(AppErrorCode.Forbidden, "Teacher is not assigned to this subject and cannot see its registrations");

           var list=await _uow.Registrations.ListActiveBySubjectAndTermWithExamAsync(subjectId,termId, ct);
            return list.Select(Mapper.TeacherRegistrationToResponse).ToList();            
        }

        public async Task<PagedResponse<StudServiceRegistrationResponse>> ListPagedAsync(
           int subjectId,
           int termId,
           int skip,
           int take,
           string? query,
           CancellationToken ct = default)
        {

            if (skip < 0) skip = 0;
            if (take <= 0) take = 20;
            if (take > 100) take = 100;

            query = query?.Trim();

            var existsSubject = await _uow.Subjects.ExistsById(subjectId, ct);
            if (!existsSubject)
                throw new AppException(AppErrorCode.NotFound, $"Subject with id {subjectId} not found.");

            var existsTerm = await _uow.Terms.ExistsById(termId, ct);
            if (!existsTerm)
                throw new AppException(AppErrorCode.NotFound, $"Term with id {termId} not found.");

            var total = await _uow.Registrations.CountAsync(subjectId, termId, query, ct);
            var list = await _uow.Registrations.ListPagedAsync(subjectId, termId, skip, take, query, ct);

            var items = list.Select(Mapper.StudServiceRegistrationToResponse).ToList();

            return new PagedResponse<StudServiceRegistrationResponse>
            {
                Items = items,
                Total = total
            };

        }
    }
}


