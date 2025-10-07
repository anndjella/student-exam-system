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
    public sealed class StudentRepository : IStudentRepository
    {
        private readonly AppDbContext _db;
        public StudentRepository(AppDbContext db) => _db = db;

        public Task<bool> ExistsByJmbgAsync(string jmbg, CancellationToken ct = default)
            => _db.Students.AnyAsync(x => x.JMBG == jmbg, ct);

        public Task<bool> ExistsByIndexAsync(string indexNumber, CancellationToken ct = default)
            => _db.Students.AnyAsync(x => x.IndexNumber == indexNumber, ct);

        public async Task<Student?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _db.Students.FirstOrDefaultAsync(x => x.ID == id, ct);

        public async Task<IReadOnlyList<Student>> ListAsync(CancellationToken ct = default)
            => await _db.Students.AsNoTracking().OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync(ct);

        public async Task<int> CreateAsync(Student student, CancellationToken ct = default)
        {
            _db.Students.Add(student);
            await _db.SaveChangesAsync(ct);
            return student.ID;
        }

        public async Task UpdateAsync(Student student, CancellationToken ct = default)
        {
              await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Student student, CancellationToken ct = default)
        {
            _db.Students.Remove(student);
            await _db.SaveChangesAsync(ct);
        }
    }
}
