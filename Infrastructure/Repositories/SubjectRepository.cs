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
    public class SubjectRepository : ISubjectRepository
    {
        private readonly AppDbContext _db;
        public SubjectRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<int> CreateAsync(Subject subject, CancellationToken ct = default)
        {
            _db.Subjects.Add(subject);
            await _db.SaveChangesAsync(ct);
            return subject.ID;
        }

        public async Task DeleteAsync(Subject subject, CancellationToken ct = default)
        {
            _db.Subjects.Remove(subject);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<Subject?> GetByIdAsync(int id, CancellationToken ct = default)
       => await _db.Subjects.FirstOrDefaultAsync(x => x.ID == id, ct);


        public async Task<IReadOnlyList<Subject>> ListAsync(CancellationToken ct = default)
       => await _db.Subjects.AsNoTracking().OrderBy(x => x.Name).ThenBy(x => x.ESPB).ToListAsync(ct);


        public async Task UpdateAsync(Subject subject, CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }

    }
}
