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
    public class SubjectRepository : BaseRepository<Subject>, ISubjectRepository
    {
        public SubjectRepository(AppDbContext db) : base(db) { }

        public Task<bool> ExistsByCode(string code, CancellationToken ct)
         => Set.AsNoTracking().AnyAsync(x => x.Name.ToLower() == code.ToLower(), ct);
        public Task<bool> ExistsById(int id, CancellationToken ct)
       => ExistsByIdAsync(id, ct);
        public Task<Subject?> GetByIdWithTeachersAsync(int id, CancellationToken ct = default)
        =>Set.Include(e=>e.TeachingAssignments).ThenInclude(e=>e.Teacher).FirstOrDefaultAsync(x=>x.ID == id, ct);
        public Task<Subject?> GetByCodeWithTeachersAsync(string code, CancellationToken ct = default)
        => Set.Include(e => e.TeachingAssignments).ThenInclude(e => e.Teacher).FirstOrDefaultAsync(x => x.Code == code, ct);

        public Task<List<int>> GetExistingIdsAsync(List<int> ids, CancellationToken ct)
        => Set.Where(s => ids.Contains(s.ID)).Select(s => s.ID).ToListAsync(ct);
        public Task<List<Subject>> ListAllIncludingInactiveAsync(CancellationToken ct = default) // for student service to make enrollments
        => Set.AsNoTracking().ToListAsync(ct);
        public Task<int> CountAdminAsync(bool isActive, string? query, CancellationToken ct = default)
        {
            query = query?.Trim();

            IQueryable<Subject> q = Set.IgnoreQueryFilters();

            q = q.Where(s => s.IsActive == isActive);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var like = $"%{query}%";
                q = q.Where(s =>
                    EF.Functions.Like(s.Code, like) ||
                    EF.Functions.Like(s.Name, like));
            }

            return q.CountAsync(ct);
        }

        public Task<List<Subject>> ListPagedWithTeachersAsync(int skip, int take, bool isActive, string? query, CancellationToken ct = default)
        {
            query = query?.Trim();

            IQueryable<Subject> q = Set
                .IgnoreQueryFilters()
                .Include(s => s.TeachingAssignments)
                    .ThenInclude(ta => ta.Teacher)
                .AsNoTracking()
                .Where(s => s.IsActive == isActive);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var like = $"%{query}%";
                q = q.Where(s => EF.Functions.Like(s.Code, like) || EF.Functions.Like(s.Name, like));
            }

            return q.OrderBy(s => s.Code)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }


        public Task<Subject?> GetByCodeAsync(string subjectCode, CancellationToken ct)
        =>Set.AsNoTracking().FirstOrDefaultAsync(e=>e.Code==subjectCode, ct);
    }
}
