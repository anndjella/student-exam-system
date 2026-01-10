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
            int schoolYearId,
            List<int> subjectIds,
            CancellationToken ct)
        {
            var pairs = await _db.Enrollments
                .Where(e => studentIds.Contains(e.StudentID)
                            && e.SchoolYearID == schoolYearId
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
        .Include(e => e.SchoolYear)
        .ToListAsync(ct);

        public Task<List<Enrollment>> ListActiveAsync(DateOnly today, CancellationToken ct)
        {
            return _db.Enrollments
                .Include(e => e.SchoolYear)
                .Where(e => e.Status == EnrollmentStatus.Active)
                .Where(e => e.SchoolYear.EndDate < today)
                .ToListAsync(ct);
        }
    }
}
