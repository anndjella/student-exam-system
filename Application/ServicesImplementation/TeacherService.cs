using Application.Common;
using Application.DTO.Students;
using Application.DTO.Teachers;
using Application.Services;
using Domain.Entity;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ServicesImplementation
{
    public class TeacherService : ITeacherService
    {
        private readonly ITeacherRepository _repo;
        public TeacherService(ITeacherRepository repo) => _repo = repo;
        public async Task<TeacherResponse> CreateAsync(CreateTeacherRequest req, CancellationToken ct = default)
        {
            if (await _repo.ExistsByJmbgAsync(req.JMBG, ct))
                throw new AppException(AppErrorCode.Conflict,"Teacher with this JMBG already exists.");

            Teacher teacher = new Teacher
            {
                JMBG = req.JMBG,
                FirstName = req.FirstName,
                LastName = req.LastName,
                DateOfBirth = req.DateOfBirth,
                Title=req.Title
            };

            var id = await _repo.CreateAsync(teacher, ct);
            var created = await _repo.GetByIdAsync(id, ct) ?? 
                throw new AppException(AppErrorCode.Unexpected,"Unexpected error in creating.");

            return Map(created);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var s = await _repo.GetByIdAsync(id, ct);
            if (s is null) 
                throw new AppException(AppErrorCode.NotFound, $"Teacher with id {id} not found.");
            await _repo.DeleteAsync(s, ct);
        }

        public async Task<TeacherResponse?> GetAsync(int id, CancellationToken ct = default)
        {
            var s = await _repo.GetByIdAsync(id, ct);
            return s is null ? 
                throw new AppException(AppErrorCode.NotFound, $"Teacher with id {id} not found.")
                : Map(s);
        }

        public async Task<IReadOnlyList<TeacherResponse>> ListAsync(CancellationToken ct = default)
        {
            var list = await _repo.ListAsync(ct);
            return list.Select(Map).ToList();
        }

        public async Task UpdateAsync(int id, UpdateTeacherRequest req, CancellationToken ct = default)
        {
            var s = await _repo.GetByIdAsync(id, ct) ??
                throw new AppException(AppErrorCode.NotFound, $"Teacher with id {id} not found.");

            if (req.FirstName is not null) s.FirstName = req.FirstName;
            if (req.LastName is not null) s.LastName = req.LastName;
            if (req.Title is not null) s.Title = req.Title.Value;

            await _repo.UpdateAsync(s, ct);
        }
        private static TeacherResponse Map(Teacher s) => new()
        {
            Id = s.ID,
            FirstName = s.FirstName,
            LastName = s.LastName,
            Title = s.Title
        };
    }
}
