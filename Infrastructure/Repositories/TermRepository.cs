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
    public sealed class TermRepository : ITermRepository
    {
        private readonly AppDbContext _db;
        public TermRepository(AppDbContext db) { _db = db; }

        public void Add(Term term)
        =>_db.Terms.Add(term);

        public Task<Term?> GetByIdAsync(int termId, CancellationToken ct)
        => _db.Terms.FirstOrDefaultAsync(e=>e.ID == termId, ct);
        public Task<bool> ExistsOverlapAsync(DateOnly start, DateOnly end,int? excludeId, CancellationToken ct)
        => _db.Terms.AnyAsync
            (t => (excludeId == null || t.ID != excludeId) &&
             t.StartDate <= end &&
             t.EndDate >= start,
             ct);
        public void Remove(Term term)
        =>_db.Terms.Remove(term);

        public Task<List<Term>> ListAllAsync(CancellationToken ct)
     => _db.Terms
         .OrderByDescending(t => t.StartDate)
         .ToListAsync(ct);

        public Task<List<Term>> ListCurrentAndFutureAsync(DateOnly today, CancellationToken ct)
            => _db.Terms
                .Where(t => (t.StartDate <= today && t.EndDate >= today) || t.StartDate>today)
                .OrderBy(t => t.StartDate)
                .ToListAsync(ct);

        public Task<List<Term>> ListOpenForRegistrationAsync(DateOnly today, CancellationToken ct)
         => _db.Terms
            .Where(t => t.RegistrationStartDate <= today && t.RegistrationEndDate >= today)
            .ToListAsync(ct);
        public async Task<List<Term>> ListCurrentAndPrev2Async(DateOnly today, CancellationToken ct)
        {
            var current = await _db.Terms
                .Where(t => t.StartDate <= today && t.EndDate >= today)
                .OrderByDescending(t => t.StartDate)
                .Take(1)
                .ToListAsync(ct);

            var prev2 = await _db.Terms
                .Where(t => t.EndDate < today)
                .OrderByDescending(t => t.EndDate)
                .Take(2)
                .ToListAsync(ct);

            return current
                .Concat(prev2)
                .OrderByDescending(t => t.StartDate)
                .ToList();
        }

    }
}
