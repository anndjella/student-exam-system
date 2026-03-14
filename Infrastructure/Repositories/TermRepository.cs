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
        public async Task<List<Term>> ListForTeacherGradingAsync(int teacherId,int subjectId, CancellationToken ct)
        {
            var terms = await _db.Registrations
            .Where(r => r.SubjectID == subjectId)
            .Where(r=>r.IsActive)
            .Where(r => r.Exam == null || r.Exam.SignedAt == null)
            .Select(r => r.Term)
            .Distinct()
            .OrderByDescending(t => t.StartDate)
            .ToListAsync(ct);

            if (terms.Count > 0)
                return terms;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var currentTerm = await _db.Terms
                .Where(t => t.StartDate <= today && t.EndDate >= today)
                .OrderByDescending(t => t.StartDate)
                .FirstOrDefaultAsync(ct);

            if (currentTerm is not null)
                return new List<Term> { currentTerm };

            var lastTerm = await _db.Terms
                .Where(t => t.EndDate < today)
                .OrderByDescending(t => t.EndDate)
                .FirstOrDefaultAsync(ct);

            if (lastTerm is not null)
                return new List<Term> { lastTerm };

            return new List<Term>();
        }
        public async Task<Term?> GetPreviousTermAsync(int currentTermId, CancellationToken ct = default)
        {
            var current = await _db.Terms
                .Where(t => t.ID == currentTermId)
                .Select(t => new { t.ID, t.StartDate, t.EndDate })
                .FirstOrDefaultAsync(ct);

            if (current is null)
                return null;

            return await _db.Terms
                .Where(t => t.StartDate < current.StartDate)
                .OrderByDescending(t => t.StartDate)
                .ThenByDescending(t => t.EndDate)
                .FirstOrDefaultAsync(ct);
        }
    }
}
