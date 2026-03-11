using Domain.Entity;
using Domain.Enums;
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
        public Task<List<Enrollment>> ListByStudentIdWithSubjectAndTeachersAsync(int studentId, CancellationToken ct)
            => _db.Enrollments
                .AsTracking()
                .Where(e => e.StudentID == studentId)
                .Include(e => e.Subject)
                .ThenInclude(e=>e.TeachingAssignments)
                .ThenInclude(e=>e.Teacher)
                .ToListAsync(ct);

        public Task<List<Enrollment>> ListPagedAsync(int skip, int take, string? query, CancellationToken ct = default)
        {
            query = query?.Trim();

            IQueryable<Enrollment> q = _db.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Subject);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var like = $"%{query}%";

                q = q.Where(e =>
                    EF.Functions.Like(e.Student.IndexNumber, like) ||
                    EF.Functions.Like(e.Subject.Code, like));
            }

            return q.AsNoTracking()
                .OrderBy(e => e.Student.IndexNumber)
                .ThenBy(e => e.Subject.Code)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }

        public Task<List<Enrollment>> ListPagedByStudentAsync(int studentId,int skip,int take,string? query, CancellationToken ct = default)
        {
            query = query?.Trim();

            IQueryable<Enrollment> q = _db.Enrollments
                .Where(e => e.StudentID == studentId)
                .Include(e => e.Student)
                .Include(e => e.Subject);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var like = $"%{query}%";
                q = q.Where(e => EF.Functions.Like(e.Subject.Code, like));
            }

            return q.AsNoTracking()
                .OrderBy(e => e.Subject.Code)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }
        public Task<List<Enrollment>> ListPagedBySubjectAsync(int subjectId,int skip,int take, string? query, CancellationToken ct = default)
        {
            query = query?.Trim();

            IQueryable<Enrollment> q = _db.Enrollments
                .Where(e => e.SubjectID == subjectId)
                .Include(e => e.Student)
                .Include(e => e.Subject);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var like = $"%{query}%";
                q = q.Where(e =>EF.Functions.Like(e.Student.IndexNumber, like));
            }

            return q.AsNoTracking()
                .OrderBy(e => e.Student.IndexNumber)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }

        public Task<int> CountByStudentAsync(int studentId, string? query, CancellationToken ct = default)
        {
            query = query?.Trim();

            IQueryable<Enrollment> q = _db.Enrollments
                .Where(e => e.StudentID == studentId);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var like = $"%{query}%";
                q = q.Where(e =>
                    EF.Functions.Like(e.Student.IndexNumber, like) ||
                    EF.Functions.Like(e.Subject.Code, like));
            }

            return q.CountAsync(ct);
        }

        public Task<int> CountBySubjectAsync(int subjectId, string? query, CancellationToken ct = default)
        {
            query = query?.Trim();

            IQueryable<Enrollment> q = _db.Enrollments
                .Where(e => e.SubjectID == subjectId);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var like = $"%{query}%";
                q = q.Where(e =>
                    EF.Functions.Like(e.Student.IndexNumber, like) ||
                    EF.Functions.Like(e.Subject.Code, like));
            }

            return q.CountAsync(ct);
        }

        public Task<bool> ExistsAsync(int studentId, int subjectId, CancellationToken ct)
             => _db.Enrollments.AnyAsync(e => e.StudentID == studentId && e.SubjectID == subjectId);

        public Task<bool> IsPassedAsync(int studentId, int subjectId, CancellationToken ct)
             => _db.Enrollments.AnyAsync(e => e.StudentID == studentId && e.SubjectID == subjectId && e.IsPassed);

        public Task<bool> ExistsBySubjectIdAsync(int subjectId, CancellationToken ct)
        => _db.Enrollments.AsNoTracking().AnyAsync(e => e.SubjectID == subjectId);

        public Task<List<Enrollment>> ListNotPassed(int studentId, CancellationToken ct)
        => _db.Enrollments.Where(e=>e.StudentID==studentId && !e.IsPassed).Include(e => e.Subject).ThenInclude(e => e.TeachingAssignments)
                .ThenInclude(e => e.Teacher).ToListAsync();

        public Task<List<Enrollment>> ListByStudentsAndSubjectAsync(List<int> studentIds, int subjectId, CancellationToken ct)
        => _db.Enrollments
            .Where(e =>e.SubjectID == subjectId && studentIds.Contains(e.StudentID))
            .ToListAsync(ct);

        public void Remove(Enrollment enrollment)
        => _db.Enrollments.Remove(enrollment);
        public void Add(Enrollment enrollment)
        => _db.Enrollments.Add(enrollment);

        public Task<Enrollment?> GetAsync(int studentId, int subjectId, CancellationToken ct)
        =>_db.Enrollments.FirstOrDefaultAsync(e=>e.StudentID==studentId &&e.SubjectID==subjectId);

        
    }
}
