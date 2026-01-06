using Domain.Entity;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infrastructure.Repositories
{
    public class ExamRepository : IExamRepository
    {
        private readonly AppDbContext _db;
        public ExamRepository(AppDbContext db) => _db = db;
        public async Task<Exam?> GetByKeyAsync(int studentId, int subjectId, DateOnly date, CancellationToken ct = default)
        => await _db.Exams.FirstOrDefaultAsync(x =>
            x.StudentID == studentId &&
            x.SubjectID == subjectId &&
            x.Date == date, ct);

        public Task<Exam?> GetByKeyWithDetailsAsync(int studentId, int subjectId, DateOnly date, CancellationToken ct = default)
            => _db.Exams
            .FirstOrDefaultAsync(e =>
               e.StudentID == studentId &&
               e.SubjectID == subjectId &&
               e.Date == date, ct);

        public async Task<IReadOnlyList<Exam>> ListWithDetailsAsync(CancellationToken ct = default)
            => await _db.Exams
                .AsNoTracking()
                .OrderByDescending(x => x.Date).ThenBy(x => x.StudentID)
                .ToListAsync(ct);

        public async Task CreateAsync(Exam exam, CancellationToken ct = default)
        {
            _db.Exams.Add(exam);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Exam exam, CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int studentId, int subjectId, DateOnly date, CancellationToken ct = default)
        {
            var exam = await GetByKeyAsync(studentId, subjectId, date, ct);
            if (exam is null) return;
            _db.Exams.Remove(exam);
            await _db.SaveChangesAsync(ct);
        }
        public async Task<bool> ExistsOnDateAsync(int studentId, int subjectId, DateOnly date, CancellationToken ct = default)
        {
            return await _db.Exams.AnyAsync(e =>
             e.StudentID == studentId &&
             e.SubjectID == subjectId &&
             e.Date == date, ct);
        }
        public async Task<bool> HasPassedAsync(int studentId, int subjectId, CancellationToken ct = default)
        {
            return await _db.Exams.AnyAsync(e => e.StudentID == studentId
                                   && e.SubjectID == subjectId
                                   && e.Grade >= 6, ct);
        }
    }
}
