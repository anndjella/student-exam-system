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
    public sealed class StudentStatsRepository : IStudentStatsRepository
    {
        private readonly AppDbContext _db;
        public StudentStatsRepository(AppDbContext db)
        {
            _db = db ;
        }
        public Task<StudentStats?> GetByStudentIdAsync(int studentId, CancellationToken ct = default)
        =>_db.StudentStats.FirstOrDefaultAsync(e=>e.StudentID == studentId, ct);
        public Task<List<StudentStats>> ListByStudentIdsAsync(List<int> studentIds, CancellationToken ct = default)
        {
            if (studentIds is null || studentIds.Count == 0)
                return Task.FromResult(new List<StudentStats>());

            return _db.StudentStats
                .AsNoTracking()
                .Where(x => studentIds.Contains(x.StudentID))
                .ToListAsync(ct);
        }

    }
}
