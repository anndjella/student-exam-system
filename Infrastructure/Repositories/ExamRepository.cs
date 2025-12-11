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
    public class ExamRepository : IExamRepository
    {
        private readonly AppDbContext _db;
        public ExamRepository(AppDbContext db) => _db = db;
        public async Task<Exam?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Exams.FirstOrDefaultAsync(x => x.ID == id, ct);

        public Task<Exam?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
            => _db.Exams
                  .Include(x => x.Student)
                  .Include(x => x.Subject)
                  .Include(x => x.Examiner)
                  .Include(x => x.Supervisor)
                  .FirstOrDefaultAsync(x => x.ID == id, ct);

        public async Task<IReadOnlyList<Exam>> ListWithDetailsAsync(CancellationToken ct = default)
            => await _db.Exams
                .AsNoTracking()
                .Include(x => x.Student)
                .Include(x => x.Subject)
                .Include(x => x.Examiner)
                .Include(x => x.Supervisor)
                .OrderByDescending(x => x.Date).ThenBy(x => x.StudentID)
                .ToListAsync(ct);

        public async Task<int> CreateAsync(Exam exam, CancellationToken ct = default)
        {
            _db.Exams.Add(exam);
            await _db.SaveChangesAsync(ct);
            return exam.ID;
        }

        public async Task UpdateAsync(Exam exam, CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Exam exam, CancellationToken ct = default)
        {
            _db.Exams.Remove(exam);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> ExistsOnDateAsync(int studentId, int subjectId, DateOnly date, CancellationToken ct = default)
        {
           return await _db.Exams.AnyAsync(e => e.StudentID == studentId
                                  && e.SubjectID == subjectId
                                  && e.Date == date, ct);
        }

        public async Task<bool> HasPassedAsync(int studentId, int subjectId, CancellationToken ct = default)
        {
           return await _db.Exams.AnyAsync(e => e.StudentID == studentId
                                  && e.SubjectID == subjectId
                                  && e.Grade >= 6, ct);
        }
    }
}
