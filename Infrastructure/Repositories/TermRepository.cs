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
    public sealed class TermRepository : ITermRepository
    {
        private readonly AppDbContext _db;
        public TermRepository(AppDbContext db) { _db = db; }

        public void Add(Term term)
        =>_db.Terms.Add(term);

        public Task<Term?> GetByIdAsync(int termId, CancellationToken ct)
        => _db.Terms.FirstOrDefaultAsync(e=>e.ID == termId, ct);
        public Task<bool> ExistsById(int termId, CancellationToken ct)
            => _db.Terms.AsNoTracking().AnyAsync(e=>e.ID==termId, ct);
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
        public async Task<List<Term>> ListForTeacherGradingAsync(DateOnly today, CancellationToken ct)
        {
            var registrationOpen = await _db.Terms
                .Where(t => t.RegistrationStartDate <= today && t.RegistrationEndDate >= today)
                .ToListAsync(ct);

            var termActiveNow = await _db.Terms
                .Where(t => t.StartDate <= today && t.EndDate >= today)
                .ToListAsync(ct);

            var prev2 = await _db.Terms
                .Where(t => t.EndDate < today)
                .OrderByDescending(t => t.EndDate)
                .Take(2)
                .ToListAsync(ct);

            var all = registrationOpen
                .Concat(termActiveNow)
                .Concat(prev2)
                .GroupBy(t => t.ID)
                .Select(g => g.First())
                .OrderByDescending(t => t.StartDate)
                .ToList();

            return all;
        }

    }
}
