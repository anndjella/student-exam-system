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
    public class SubjectRepository : ISubjectRepository
    {
        private readonly AppDbContext _db;
        public SubjectRepository(AppDbContext db)
        {
            _db = db;
        }
        public void Add(Subject subject)
        =>_db.Subjects.Add(subject);

        public Task<bool> ExistsByCode(string code, CancellationToken ct)
         => _db.Subjects.AsNoTracking().AnyAsync(x => x.Name.ToLower() == code.ToLower(), ct);
        public Task<bool> ExistsById(int id, CancellationToken ct)
       => _db.Subjects.AsNoTracking().AnyAsync(x => x.ID==id, ct);
        public Task<Subject?> GetByIdWithTeachersAsync(int id, CancellationToken ct = default)
        =>_db.Subjects.Include(e=>e.TeachingAssignments).ThenInclude(e=>e.Teacher).FirstOrDefaultAsync(x=>x.ID == id, ct);
        public Task<Subject?> GetByCodeWithTeachersAsync(string code, CancellationToken ct = default)
        => _db.Subjects.Include(e => e.TeachingAssignments).ThenInclude(e => e.Teacher).FirstOrDefaultAsync(x => x.Code == code, ct);

        public Task<List<int>> GetExistingIdsAsync(List<int> ids, CancellationToken ct)
        => _db.Subjects.Where(s => ids.Contains(s.ID)).Select(s => s.ID).ToListAsync(ct);
        public Task<List<Subject>> ListActiveAsync(CancellationToken ct = default) // for student service to make enrollments
        => _db.Subjects.AsNoTracking().Where(x => x.IsActive).ToListAsync(ct);
        public void Remove(Subject subject)
        => _db.Subjects.Remove(subject);

        //public Task<List<Subject>> ListAllWithTeachersAsync(CancellationToken ct = default)
        //=> _db.Subjects.AsNoTracking()
        //      .Include(e => e.TeachingAssignments)
        //    .ThenInclude(e => e.Teacher).ToListAsync(ct);
        public Task<int> CountAdminAsync(bool isActive, string? query, CancellationToken ct = default)
        {
            query = query?.Trim();

            IQueryable<Subject> q = _db.Subjects;

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

            IQueryable<Subject> q = _db.Subjects
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
        =>_db.Subjects.AsNoTracking().FirstOrDefaultAsync(e=>e.Code==subjectCode, ct);
    }
}
