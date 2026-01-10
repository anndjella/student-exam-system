using Application.Common;
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
            Subject subject = new Subject
            {
                Name = req.Name,
                ECTS = req.ECTS
            };
            _uow.Subjects.Add(subject);
            await _uow.CommitAsync(ct);
           
            var created = await _uow.Subjects.GetByIdAsync(subject.ID, ct) ??
                throw new AppException(AppErrorCode.Unexpected, "Unexpected error in creating.");

            return Mapper.SubjectToResponse(created);
        }

        public async Task SoftDeleteAsync(int id, CancellationToken ct = default)
        {
            var s = await _uow.Subjects.GetByIdAsync(id, ct);
            if (s is null) 
                throw new AppException(AppErrorCode.NotFound, $"Subject with id {id} not found.");

            s.MarkDeleted();
            //_uow.Subjects.Update(s);
           await  _uow.CommitAsync(ct);
        }
        public async Task<SubjectResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var s = await _uow.Subjects.GetByIdAsync(id, ct);
            return s is null ?
                throw new AppException(AppErrorCode.NotFound, $"Subject with id {id} not found.")
                : Mapper.SubjectToResponse(s);
        }

        public async Task UpdateAsync(int id, UpdateSubjectRequest req, CancellationToken ct = default)
        {
            var s = await _uow.Subjects.GetByIdAsync(id, ct) ??
                throw new AppException(AppErrorCode.NotFound, $"Subject with id {id} not found.");

            if (req.Name is not null) s.Name = req.Name;
            if (req.ECTS is not null) s.ECTS = req.ECTS.Value;

            //_uow.Subjects.Update(s);
            await _uow.CommitAsync(ct);
        }

        public async Task<SubjectResponse?> GetByNameAsync(string name, CancellationToken ct = default)
        {
            var s = await _uow.Subjects.GetByNameAsync(name, ct);
            return s is null ?
                throw new AppException(AppErrorCode.NotFound, $"Subject named {name} not found.")
                : Mapper.SubjectToResponse(s);
        }
    }
}
