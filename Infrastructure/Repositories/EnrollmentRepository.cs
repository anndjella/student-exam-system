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
    public sealed class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly AppDbContext _db;
        public EnrollmentRepository(AppDbContext db) { _db = db; }
        public async Task<HashSet<(int StudentId, int SubjectId)>> ListExistingPairsAsync(
            List<int> studentIds,
            List<int> subjectIds,
            CancellationToken ct)
        {
            var pairs = await _db.Enrollments
                .Where(e => studentIds.Contains(e.StudentID)
                            && subjectIds.Contains(e.SubjectID))
                .Select(e => new { e.StudentID, e.SubjectID })
                .ToListAsync(ct);

            return pairs.Select(x => (x.StudentID, x.SubjectID)).ToHashSet();
        }

        public void AddRange(IEnumerable<Enrollment> enrollments)
            => _db.Enrollments.AddRange(enrollments);
        public Task<List<Enrollment>> ListByStudentIdAsync(int studentId, CancellationToken ct)
            => _db.Enrollments
                .AsTracking()
                .Where(e => e.StudentID == studentId)
                .Include(e => e.Subject)
                .ToListAsync(ct);

        public Task<Enrollment?> GetAsync(int studentId, int subjectId, CancellationToken ct)
            => _db.Enrollments
                .Where(e => !e.IsPassed)
                .FirstOrDefaultAsync(e => e.StudentID == studentId && e.SubjectID == subjectId);

        public Task<bool> ExistsAsync(int studentId, int subjectId, CancellationToken ct)
             => _db.Enrollments.AnyAsync(e => e.StudentID == studentId && e.SubjectID == subjectId);

        public Task<bool> IsPassedAsync(int studentId, int subjectId, CancellationToken ct)
             => _db.Enrollments.AnyAsync(e => e.StudentID == studentId && e.SubjectID == subjectId && e.IsPassed);
    }
}
