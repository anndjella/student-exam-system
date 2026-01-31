using Application.DTO.Exams;
using Domain.Entity;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class TeacherRepository : ITeacherRepository
    {
        private readonly AppDbContext _db;
        public TeacherRepository(AppDbContext db)
        {
            _db = db;
        }
        public void Add(Teacher teacher) => _db.Teachers.Add(teacher);
        public Task<bool> ExistsByEmployeeNumAsync(string employeeNum, CancellationToken ct = default)
          => _db.Teachers.AnyAsync(x => x.EmployeeNumber == employeeNum, ct);
        public Task<bool> ExistsByIdAsync(int id, CancellationToken ct = default)
          => _db.Teachers.AsNoTracking().AnyAsync(x => x.ID == id, ct);
        public Task DeleteByIdAsync(int teacherId, CancellationToken ct = default)
        => _db.Teachers.Where(e => e.ID == teacherId).ExecuteDeleteAsync();

        public  Task<Teacher?> GetByIdAsync(int id, CancellationToken ct = default)
           =>  _db.Teachers.FirstOrDefaultAsync(x => x.ID == id, ct);
       
        public Task<Teacher?> GetByIdWithUserAsync(int id, CancellationToken ct = default)
        =>_db.Teachers
            .Include(x=>x.User)
            .FirstOrDefaultAsync(x=>x.ID == id, ct);

        public Task<Teacher?> GetByEmployeeNumAsync(string employeeNum, CancellationToken ct = default)
        => _db.Teachers.FirstOrDefaultAsync(x => x.EmployeeNumber == employeeNum);
        public Task<int> CountAsync(string? query, CancellationToken ct = default)
        {
            query = query?.Trim();

            IQueryable<Teacher> q = _db.Teachers;

            q = q.Where(t => !t.IsDeleted);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var like = $"%{query}%";
                q = q.Where(t =>
                    EF.Functions.Like(t.FirstName, like) ||
                    EF.Functions.Like(t.LastName, like) ||
                    EF.Functions.Like(t.EmployeeNumber, like));
            }

            return q.CountAsync(ct);
        }

        public Task<List<Teacher>> ListPagedAsync(int skip, int take, string? query, CancellationToken ct = default)
        {
            query = query?.Trim();

            IQueryable<Teacher> q = _db.Teachers;

            q = q.Where(t => !t.IsDeleted);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var like = $"%{query}%";
                q = q.Where(t =>
                    EF.Functions.Like(t.FirstName, like) ||
                    EF.Functions.Like(t.LastName, like) ||
                    EF.Functions.Like(t.EmployeeNumber, like));
            }

            return q.AsNoTracking()
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .ThenBy(t => t.EmployeeNumber)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }
    }
}
