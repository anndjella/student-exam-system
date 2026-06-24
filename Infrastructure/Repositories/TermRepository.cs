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
    public sealed class TermRepository : BaseRepository<Term>, ITermRepository
    {
        public TermRepository(AppDbContext db) : base(db) { }

        public Task<bool> ExistsById(int termId, CancellationToken ct)
            => ExistsByIdAsync(termId, ct);
        public Task<bool> ExistsOverlapAsync(DateOnly start, DateOnly end,int? excludeId, CancellationToken ct)
        => Set.AnyAsync
            (t => (excludeId == null || t.ID != excludeId) &&
             t.StartDate <= end &&
             t.EndDate >= start,
             ct);

        public Task<List<Term>> ListAllAsync(CancellationToken ct)
     => Set
         .OrderByDescending(t => t.StartDate)
         .ToListAsync(ct);

        public Task<List<Term>> ListCurrentAndFutureAsync(DateOnly today, CancellationToken ct)
            => Set
                .Where(t => (t.StartDate <= today && t.EndDate >= today) || t.StartDate>today)
                .OrderBy(t => t.StartDate)
                .ToListAsync(ct);

        public Task<List<Term>> ListOpenForRegistrationAsync(DateOnly today, CancellationToken ct)
         => Set
            .Where(t => t.RegistrationStartDate <= today && t.RegistrationEndDate >= today)
            .ToListAsync(ct);
        public async Task<List<Term>> ListForTeacherGradingAsync(int teacherId,int subjectId, CancellationToken ct)
        {
            var terms = await Db.Registrations
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

            var currentTerm = await Set
                .Where(t => t.StartDate <= today && t.EndDate >= today)
                .OrderByDescending(t => t.StartDate)
                .FirstOrDefaultAsync(ct);

            if (currentTerm is not null)
                return new List<Term> { currentTerm };

            var lastTerm = await Set
                .Where(t => t.EndDate < today)
                .OrderByDescending(t => t.EndDate)
                .FirstOrDefaultAsync(ct);

            if (lastTerm is not null)
                return new List<Term> { lastTerm };

            return new List<Term>();
        }
        public async Task<Term?> GetPreviousTermAsync(int currentTermId, CancellationToken ct = default)
        {
            var current = await Set
                .Where(t => t.ID == currentTermId)
                .Select(t => new { t.ID, t.StartDate, t.EndDate })
                .FirstOrDefaultAsync(ct);

            if (current is null)
                return null;

            return await Set
                .Where(t => t.StartDate < current.StartDate)
                .OrderByDescending(t => t.StartDate)
                .ThenByDescending(t => t.EndDate)
                .FirstOrDefaultAsync(ct);
        }
    }
}
