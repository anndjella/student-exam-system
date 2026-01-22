using Domain.Entity;
using Domain.Enums;
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
    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly AppDbContext _db;
        public RegistrationRepository(AppDbContext db) => _db = db;

        public void Add(Registration r) => _db.Registrations.Add(r);

        public Task<Registration?> GetAsync(int studentId, int subjectId, int termId, CancellationToken ct = default)
            => _db.Registrations.FirstOrDefaultAsync(x =>
                x.StudentID == studentId && x.SubjectID == subjectId && x.TermID == termId, ct);

        public Task<bool> ExistsActiveAsync(int studentId, int subjectId, int termId, CancellationToken ct = default)
            => _db.Registrations.AnyAsync(x =>
                x.StudentID == studentId &&
                x.SubjectID == subjectId &&
                x.TermID == termId &&
                x.IsActive, ct);

        public Task<List<Registration>> ListActiveByStudentAsync(int studentId, CancellationToken ct = default)
        => _db.Registrations.Where(x => x.StudentID == studentId)
                .Include(x=>x.Subject)
                .Include(x=>x.Term)
                .OrderByDescending(x => x.RegisteredAt)
                .ToListAsync(ct);
        public Task<List<int>> ListActiveStudentIdsAsync(int subjectId, int termId, CancellationToken ct = default)
         => _db.Registrations
                .Where(r => r.SubjectID == subjectId && r.TermID == termId && r.IsActive)
                .Select(r => r.StudentID)
                .Distinct()
                .ToListAsync(ct);
        public Task<List<Registration>> ListActiveForStudentAsync(int studentId, CancellationToken ct = default)
            => _db.Registrations
                    .Where(e => e.StudentID == studentId && e.IsActive)
                    .Include(e=>e.Subject).Include(e=>e.Term)
                    .ToListAsync();

        public Task<bool> ExistsAnyForTermAsync(int termId, CancellationToken ct = default)
        => _db.Registrations.AnyAsync(e => e.TermID == termId && e.IsActive);

        public Task<bool> ExistsAnyForSubjectAsync(int subjectId, CancellationToken ct)
        => _db.Registrations.AsNoTracking().AnyAsync(e => e.SubjectID == subjectId);

       public Task<List<Registration>> ListActiveBySubjectAndTermAsync(int subjectId, int termId, CancellationToken ct = default)
            => _db.Registrations
            .Where(e => e.SubjectID == subjectId && e.TermID == termId && e.IsActive)
            .Include(e=>e.Student)
            .ToListAsync(ct);


    }
}
