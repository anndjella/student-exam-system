using Application.DTO.Exams;
using Domain.Entity;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class TeacherRepository : BaseRepository<Teacher>, ITeacherRepository
    {
        public TeacherRepository(AppDbContext db) : base(db) { }

        public Task<bool> ExistsByEmployeeNumAsync(string employeeNum, CancellationToken ct = default)
          => Set.IgnoreQueryFilters().AnyAsync(x => x.EmployeeNumber == employeeNum, ct);

        public Task DeleteByIdAsync(int teacherId, CancellationToken ct = default)
        => Set.Where(e => e.ID == teacherId).ExecuteDeleteAsync();
       
        public Task<Teacher?> GetByIdWithUserAsync(int id, CancellationToken ct = default)
        =>Set
            .Include(x=>x.User)
            .FirstOrDefaultAsync(x=>x.ID == id, ct);

        public Task<Teacher?> GetByEmployeeNumAsync(string employeeNum, CancellationToken ct = default)
        => Set.FirstOrDefaultAsync(x => x.EmployeeNumber == employeeNum);
        public Task<int> CountAsync(string? query,bool onlyDeleted, CancellationToken ct = default)
        {
            query = query?.Trim();

            IQueryable<Teacher> q = Set;

            if (onlyDeleted)
            {
                q = q.IgnoreQueryFilters().Where(s => s.IsDeleted);
            }
            else
            {
                q = q.Where(s => !s.IsDeleted);
            }

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

        public Task<List<Teacher>> ListPagedAsync(int skip, int take, string? query, bool onlyDeleted, CancellationToken ct = default)
        {
            query = query?.Trim();

            IQueryable<Teacher> q = Set;

            if (onlyDeleted)
            {
                q = q.IgnoreQueryFilters().Where(s => s.IsDeleted);
            }
            else
            {
                q = q.Where(s => !s.IsDeleted);
            }

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
