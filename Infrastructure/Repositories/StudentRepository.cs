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
    public sealed class StudentRepository : IStudentRepository
    {   
        private readonly AppDbContext _db;
        public StudentRepository(AppDbContext db) => _db = db;

        public void Add(Student s) => _db.Students.Add(s);
        public Task<Student?> GetByIdAsync(int id, CancellationToken ct = default)
            => _db.Students.FirstOrDefaultAsync(x => x.ID == id, ct);

        public Task<bool> ExistsByIndexAsync(string indexNumber, CancellationToken ct = default)
            => _db.Students.AnyAsync(x => x.IndexNumber == indexNumber, ct);

        public Task<Student?> GetByIndexAsync(string indexNumber, CancellationToken ct = default)
          => _db.Students.FirstOrDefaultAsync(x => x.IndexNumber == indexNumber, ct);

        public Task<Student?> GetByIdWithUserAsync(int id, CancellationToken ct = default)
         => _db.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(x => x.ID == id, ct);
        public Task<List<int>> ListIdsByIndexPrefixAsync(string prefix, CancellationToken ct)
         => _db.Students
            .Where(s => s.IndexNumber.StartsWith(prefix))
            .Select(s => s.ID)
            .ToListAsync(ct);

        public Task<int> CountAsync(string? query, CancellationToken ct = default)
        {
            query = query?.Trim();

            var q = _db.Students.AsQueryable();

            q = q.Where(s => !s.IsDeleted);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var like = $"%{query}%";
                q = q.Where(s =>
                    EF.Functions.Like(s.FirstName, like) ||
                    EF.Functions.Like(s.LastName, like) ||
                    EF.Functions.Like(s.IndexNumber, like));
            }

            return q.CountAsync(ct);
        }

        public Task<List<Student>> ListPagedAsync(int skip, int take, string? query, CancellationToken ct = default)
        {
            query = query?.Trim();

            var q = _db.Students.AsQueryable();

            q = q.Where(s => !s.IsDeleted);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var like = $"%{query}%";
                q = q.Where(s =>
                    EF.Functions.Like(s.FirstName, like) ||
                    EF.Functions.Like(s.LastName, like) ||
                    EF.Functions.Like(s.IndexNumber, like));
            }

            return q.AsNoTracking()
                .OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ThenBy(s => s.IndexNumber)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }

    }
}
