using Application.Common;
using Application.DTO.Registrations;
using Application.Services;
using Domain.Entity;
using Domain.Enums;
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
        public RegistrationService(IUnitOfWork uow) => _uow = uow;

        private async Task<DateTime> ValidateRegistrationPreconditionsAsync(int studentId, int subjectId, int termId, CancellationToken ct)
        {
            var term = await _uow.Terms.GetByIdAsync(termId, ct)
                ?? throw new AppException(AppErrorCode.NotFound, $"Term {termId} not found.");

            var now = DateTime.UtcNow;

            if (!term.IsInRegistrationWindow(now))
                throw new AppException(AppErrorCode.Conflict, "Registration window is closed.");

            if (!await _uow.Enrollments.ExistsAsync(studentId, subjectId, ct))
                throw new AppException(AppErrorCode.Validation, "Student is not enrolled in this subject");

            if (await _uow.Enrollments.IsPassedAsync(studentId, subjectId, ct))
                throw new AppException(AppErrorCode.Validation, "Student already passed this subject");

            return now;
        }

        public async Task<StudentRegistrationResponse> CreateAsync(int studentId, CreateRegistrationRequest req, CancellationToken ct = default)
        {
            var now= await ValidateRegistrationPreconditionsAsync(studentId,req.SubjectID, req.TermID, ct);

            var existing = await _uow.Registrations.GetAsync(studentId, req.SubjectID, req.TermID, ct);

            if (existing is null)
            {
                var r = new Registration
                {
                    StudentID = studentId,
                    SubjectID = req.SubjectID,
                    TermID = req.TermID,
                    IsActive = true,
                    RegisteredAt = now,
                    CancelledAt = null
                };

                _uow.Registrations.Add(r);
                await _uow.CommitAsync(ct);

                return Mapper.StudentRegistrationToResponse(r);
            }

            if (existing.IsActive)
                throw new AppException(AppErrorCode.Conflict, "Subject is already registered for this term.");

            existing.IsActive = true;
            existing.RegisteredAt = now;
            existing.CancelledAt = null;

            await _uow.CommitAsync(ct);
            return Mapper.StudentRegistrationToResponse(existing);
        }

        public async Task CancelAsync(int studentId, int subjectId, int termId, CancellationToken ct = default)
        {
            var now = await ValidateRegistrationPreconditionsAsync(studentId, subjectId, termId, ct);

            var existing = await _uow.Registrations.GetAsync(studentId, subjectId, termId, ct)
                ?? throw new AppException(AppErrorCode.NotFound, "Registration not found.");

            if (!existing.IsActive)
                throw new AppException(AppErrorCode.Conflict, "Registration is not active.");

            existing.IsActive = false;
            existing.CancelledAt = now;

            await _uow.CommitAsync(ct);
        }

        public async Task<List<StudentRegistrationResponse>> ListMyActiveAsync(int studentId, CancellationToken ct = default)
        {
            var list = await _uow.Registrations.ListActiveForStudentAsync(studentId, ct);
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


     
    }
}

