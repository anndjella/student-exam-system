using Application.Common;
using Application.DTO.Me.Teacher;
using Application.DTO.Students;
using Application.DTO.Subjects;
using Application.Services;
using Domain.Entity;
using Domain.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ServicesImplementation
{
    public class SubjectService : ISubjectService
    {
        private readonly IUnitOfWork _uow;
        public SubjectService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public async Task<SubjectResponse> CreateAsync(CreateSubjectRequest req, CancellationToken ct = default)
        {
            if (await _uow.Subjects.ExistsByCode(req.Code, ct))
                throw new AppException(AppErrorCode.Conflict, $"Subject with code {req.Code} already exists.");

            Subject subject = new Subject
            {
                Name = req.Name,
                ECTS = req.ECTS,
                Code = req.Code,
                IsActive = true
            };
            _uow.Subjects.Add(subject);
            await _uow.CommitAsync(ct);

            var created = await _uow.Subjects.GetByIdWithTeachersAsync(subject.ID, ct) ??
                throw new AppException(AppErrorCode.Unexpected, "Unexpected error in creating.");

            return Mapper.SubjectToResponse(created);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var s = await _uow.Subjects.GetByIdWithTeachersAsync(id, ct);
            if (s is null)
                throw new AppException(AppErrorCode.NotFound, $"Subject with id {id} not found.");

            if (await _uow.Registrations.ExistsAnyForSubjectAsync(id, ct))
                throw new AppException(AppErrorCode.Validation, "Subject cannot be deleted because it has registrations already.");

            if (await _uow.Exams.ExistsAnyForSubjectAsync(id, ct))
                throw new AppException(AppErrorCode.Validation, "Subject cannot be deleted because it has exams already");

            if(await _uow.Enrollments.ExistsBySubjectIdAsync(id, ct))
                throw new AppException(AppErrorCode.Validation, "Subject cannot be deleted because it has enrollments already");

            _uow.Subjects.Remove(s);
            await _uow.CommitAsync(ct);
        }
        public async Task<AdminSubjectResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var s = await _uow.Subjects.GetByIdWithTeachersAsync(id, ct);
            return s is null ?
                throw new AppException(AppErrorCode.NotFound, $"Subject with id {id} not found.")
                : Mapper.SubjectToAdminResponse(s);
        }

        public async Task<AdminSubjectResponse?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            var s = await _uow.Subjects.GetByCodeWithTeachersAsync(code, ct);
            return s is null ?
                throw new AppException(AppErrorCode.NotFound, $"Subject with code {code} not found.")
                : Mapper.SubjectToAdminResponse(s);
        }

        public async Task<AdminSubjectsResponse> ListAllWithTeachersAsync(CancellationToken ct)
        {
            var res= (await _uow.Subjects.ListAllWithTeachersAsync(ct)).Select(Mapper.SubjectToAdminResponse).ToList();
            var dto = new AdminSubjectsResponse();
            foreach(var item in res)
            {
                if(item.IsActive) dto.Active.Add(item);
                else dto.Inactive.Add(item);                   
            }
            return dto;
        }

        public async Task<List<SubjectResponse>> ListActiveAsync(CancellationToken ct=default)
        => (await _uow.Subjects.ListActiveAsync(ct)).Select(Mapper.SubjectToResponse).ToList();

        public async Task DeactivateAsync(int id, CancellationToken ct)
        {
            var res = await _uow.Subjects.GetByIdWithTeachersAsync(id, ct);
            if (res is null)
                throw new AppException(AppErrorCode.NotFound, $"Subject with id {id} not  found.");

            res.IsActive = false;
            await _uow.CommitAsync(ct);
        }
    }

}
