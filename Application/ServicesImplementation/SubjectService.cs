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
        private readonly ISubjectRepository _repo;
        public SubjectService(ISubjectRepository repo)
        {
            _repo = repo;
        }

        public async Task<SubjectResponse> CreateAsync(CreateSubjectRequest req, CancellationToken ct = default)
        {
            Subject subject = new Subject
            {
                Name=req.Name,
                ESPB=req.ESPB
            };
            var id = await _repo.CreateAsync(subject,ct);
            var created = await _repo.GetByIdAsync(id, ct) ??
                throw new AppException(AppErrorCode.Unexpected,"Unexpected error in creating.");
            return Map(created);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var s = await _repo.GetByIdAsync(id, ct);
            if (s is null) 
                throw new AppException(AppErrorCode.NotFound, $"Subject with id {id} not found.");
            await _repo.DeleteAsync(s, ct);
        }

        public async Task<SubjectResponse?> GetAsync(int id, CancellationToken ct = default)
        {
            var s = await _repo.GetByIdAsync(id, ct);
            return s is null ? 
                throw new AppException(AppErrorCode.NotFound, $"Subject with id {id} not found.")
                : Map(s);
        }

        public async Task<IReadOnlyList<SubjectResponse>> ListAsync(CancellationToken ct = default)
        {
             var list = await _repo.ListAsync(ct);
            return list.Select(Map).ToList();
        }

        public async Task UpdateAsync(int id, UpdateSubjectRequest req, CancellationToken ct = default)
        {
            var s = await _repo.GetByIdAsync(id, ct) ??
                throw new AppException(AppErrorCode.NotFound, $"Subject with id {id} not found.");

            if (req.Name is not null) s.Name = req.Name;
            if (req.ESPB is not null) s.ESPB = req.ESPB.Value;

            await _repo.UpdateAsync(s, ct);
        }
        private static SubjectResponse Map(Subject s) => new()
        {
            Id = s.ID,
            Name=s.Name,
            ESPB=s.ESPB
        };
    }
}
