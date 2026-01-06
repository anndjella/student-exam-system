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

        public Task<SubjectResponse> CreateAsync(CreateSubjectRequest req, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        //public async Task<SubjectResponse> CreateAsync(CreateSubjectRequest req, CancellationToken ct = default)
        //{
        //    Subject subject = new Subject
        //    {
        //        Name=req.Name,
        //        ECTS=req.ESPB
        //    };
        //    var id = await _repo.CreateAsync(subject,ct);
        //    var created = await _repo.GetByIdAsync(id, ct) ??
        //        throw new AppException(AppErrorCode.Unexpected,"Unexpected error in creating.");
        //    return Mapper.SubjectToResponse(created);
        //}

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var s = await _repo.GetByIdAsync(id, ct);
            if (s is null) 
                throw new AppException(AppErrorCode.NotFound, $"Subject with id {id} not found.");
            await _repo.DeleteAsync(s, ct);
        }

        public Task<SubjectResponse?> GetAsync(int id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<SubjectResponse>> ListAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(int id, UpdateSubjectRequest req, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        //public async Task<SubjectResponse?> GetAsync(int id, CancellationToken ct = default)
        //{
        //    var s = await _repo.GetByIdAsync(id, ct);
        //    return s is null ? 
        //        throw new AppException(AppErrorCode.NotFound, $"Subject with id {id} not found.")
        //        : Mapper.SubjectToResponse(s);
        //}

        //public async Task<IReadOnlyList<SubjectResponse>> ListAsync(CancellationToken ct = default)
        //{
        //     var list = await _repo.ListAsync(ct);
        //    return list.Select(Mapper.SubjectToResponse).ToList();
        //}

        //public async Task UpdateAsync(int id, UpdateSubjectRequest req, CancellationToken ct = default)
        //{
        //    var s = await _repo.GetByIdAsync(id, ct) ??
        //        throw new AppException(AppErrorCode.NotFound, $"Subject with id {id} not found.");

        //    if (req.Name is not null) s.Name = req.Name;
        //    if (req.ESPB is not null) s.ECTS = req.ESPB.Value;

        //    await _repo.UpdateAsync(s, ct);
        //}
    }
}
