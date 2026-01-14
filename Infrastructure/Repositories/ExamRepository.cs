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
    }
}
