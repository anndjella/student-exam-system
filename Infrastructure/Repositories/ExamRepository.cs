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
    public sealed class ExamRepository : IExamRepository
    {
        private readonly AppDbContext _db;
        public ExamRepository(AppDbContext db) => _db = db;

        public void Add(Exam exam) => _db.Exams.Add(exam);

        public Task<Exam?> GetByKeyAsync(int studentId, int subjectId, int termId, CancellationToken ct = default)
            => _db.Exams.FirstOrDefaultAsync(x =>
                x.StudentID == studentId && x.SubjectID == subjectId && x.TermID == termId, ct);

        public Task<List<Exam>> ListUnsignedBySubjectTermAsync(int subjectId, int termId, CancellationToken ct = default)
            => _db.Exams
                .Where(x => x.SubjectID == subjectId && x.TermID == termId && x.SignedAt == null)
                .ToListAsync(ct);
        public Task<List<Exam>> ListBySubjectTermAsync(int subjectId, int termId, CancellationToken ct = default)
          => _db.Exams
              .Where(x => x.SubjectID == subjectId && x.TermID == termId)
              .OrderBy(x => x.StudentID)
              .ToListAsync(ct);

        public Task<bool> ExistsAnyForTermAsync(int termId, CancellationToken ct = default)
         => _db.Exams.AnyAsync(e => e.TermID == termId);

        public Task<bool> ExistsSignedForTermAsync(int termId, CancellationToken ct = default)
        =>_db.Exams.AnyAsync(e=>e.TermID==termId &&  e.SignedAt != null);

        public Task<bool> ExistsAnyForSubjectAsync(int subjectId, CancellationToken ct = default)
        => _db.Exams.AsNoTracking().AnyAsync(e => e.SubjectID == subjectId);

        public Task<List<Exam>> ListUnsignedBySubjectTermWithRegistrationAsync(int subjectId, int termId, CancellationToken ct = default)
        => _db.Exams
            .Where(e => e.SubjectID == subjectId && e.TermID == termId && e.SignedAt== null)
            .Include(e=>e.Registration)
            .ToListAsync(ct);

        public Task<List<Exam>> ListSignedByStudentIdAsync(int studentId, CancellationToken ct = default)
        =>_db.Exams
              .AsNoTracking()
              .Where(e => e.StudentID == studentId && e.SignedAt != null)
              .Include(e => e.Registration)
                  .ThenInclude(r => r.Subject)
              .Include(e => e.Registration)
                  .ThenInclude(r => r.Term)
              .Include(e => e.Teacher)
              .OrderByDescending(e => e.SignedAt)
              .ToListAsync(ct);

        public Task<List<Exam>> ListAllBySubjectTermAsync(int termId, int subjectId, CancellationToken ct = default)
        => _db.Exams.AsNoTracking()
              .Where(e => e.TermID == termId && e.SubjectID==subjectId)
              .Include(e => e.Registration)
                  .ThenInclude(r => r.Subject)
            .Include(e => e.Registration)
                  .ThenInclude(r => r.Student)
              .Include(e => e.Registration)
                  .ThenInclude(r => r.Term)
              .Include(e => e.Teacher)
              .OrderByDescending(e => e.SignedAt)
              .ToListAsync(ct);
    }
}
