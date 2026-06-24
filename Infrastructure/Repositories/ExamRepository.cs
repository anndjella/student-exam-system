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
    public sealed class ExamRepository : BaseRepository<Exam>, IExamRepository
    {
        public ExamRepository(AppDbContext db) : base(db) { }

        public Task<Exam?> GetByKeyAsync(int studentId, int subjectId, int termId, CancellationToken ct = default)
            => Set.FirstOrDefaultAsync(x =>
                x.StudentID == studentId && x.SubjectID == subjectId && x.TermID == termId, ct);

        public Task<List<Exam>> ListUnsignedBySubjectTermAsync(int subjectId, int termId, CancellationToken ct = default)
            => Set
                .Where(x => x.SubjectID == subjectId && x.TermID == termId && x.SignedAt == null)
                .ToListAsync(ct);
        public Task<List<Exam>> ListBySubjectTermAsync(int subjectId, int termId, CancellationToken ct = default)
          => Set
              .Where(x => x.SubjectID == subjectId && x.TermID == termId)
              .OrderBy(x => x.StudentID)
              .ToListAsync(ct);

        public Task<bool> ExistsAnyForTermAsync(int termId, CancellationToken ct = default)
         => Set.AnyAsync(e => e.TermID == termId);

        public Task<bool> ExistsAnyForSubjectAsync(int subjectId, CancellationToken ct = default)
        => Set.AsNoTracking().AnyAsync(e => e.SubjectID == subjectId);

        public Task<List<Exam>> ListUnsignedBySubjectTermWithRegistrationAsync(int subjectId, int termId, CancellationToken ct = default)
        => Set
            .Where(e => e.SubjectID == subjectId && e.TermID == termId && e.SignedAt== null)
            .Include(e=>e.Registration)
            .ToListAsync(ct);

        public Task<List<Exam>> ListSignedByStudentIdAsync(int studentId, CancellationToken ct = default)
        =>Set
            .IgnoreQueryFilters()
              .AsNoTracking()
              .Where(e => e.StudentID == studentId && e.SignedAt != null)
              .Include(e => e.Registration)
                  .ThenInclude(r => r.Subject)
              .Include(e => e.Registration)
                  .ThenInclude(r => r.Term)
              .Include(e => e.Teacher)
              .OrderByDescending(e => e.SignedAt)
              .ToListAsync(ct);
        public Task<List<Exam>> ListAllBySubjectTermForTeacherAsync(
            int subjectId,
            int termId,
            int teacherId,
            CancellationToken ct = default)
        => Set
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(e => e.TermID == termId && e.SubjectID == subjectId && e.SignedAt != null)
            .Where(e => Db.TeachingAssignments.Any(ta =>
                ta.TeacherID == teacherId && ta.SubjectID == subjectId
            ))
            .Include(e=> e.Registration).ThenInclude(e=>e.Student)
            .Include(e => e.Teacher)
            .OrderByDescending(e => e.SignedAt)
            .ToListAsync(ct);

        public Task<bool> ExistsAnyForSubjectAndStudentAsync(int subjectId, int studentId, CancellationToken ct = default)
        => Set.AsNoTracking().AnyAsync(e => e.StudentID == studentId && e.SubjectID == subjectId);

        public Task<int> CountUnsignedBySubjectTermAsync(int subjectId, int termId, CancellationToken ct = default)
        => Set
        .AsNoTracking()
        .CountAsync(e => e.SubjectID == subjectId
                      && e.TermID == termId
                      && e.SignedAt == null, ct);

        public Task<int> CountPagedAsync(
            int subjectId,
            int termId,
            string? query,
            CancellationToken ct = default)
        {
            query = query?.Trim();

            IQueryable<Exam> q = Set
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(e => e.Registration)
                    .ThenInclude(r => r.Student)
                .Include(e => e.Registration)
                    .ThenInclude(r => r.Subject)
                .Include(e => e.Registration)
                    .ThenInclude(r => r.Term)
                .Include(e => e.Teacher);

            q = q.Where(e => e.SubjectID == subjectId && e.TermID == termId);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var like = $"%{query}%";
                q = q.Where(e => EF.Functions.Like(e.Registration.Student.IndexNumber, like));
            }

            return q.CountAsync(ct);
        }

        public async Task<List<Exam>> ListPagedAsync(
            int subjectId,
            int termId,
            int skip,
            int take,
            string? query,
            CancellationToken ct = default)
        {
            query = query?.Trim();

            IQueryable<Exam> q = Set
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(e => e.Registration)
                    .ThenInclude(r => r.Student)
                .Include(e => e.Registration)
                    .ThenInclude(r => r.Subject)
                .Include(e => e.Registration)
                    .ThenInclude(r => r.Term)
                .Include(e => e.Teacher);

            q = q.Where(e => e.SubjectID == subjectId && e.TermID == termId);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var like = $"%{query}%";
                q = q.Where(e => EF.Functions.Like(e.Registration.Student.IndexNumber, like));
            }

            return await q
                .OrderBy(e => e.Registration.Student.IndexNumber)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }

    }
}
