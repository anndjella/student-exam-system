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

        public void Add(Student s) => _db.Students.Add(s);
        public void Update(Student s) => _db.Students.Update(s);

        public Task<Student?> GetByIdAsync(int id, CancellationToken ct = default)
            => _db.Students.FirstOrDefaultAsync(x => x.ID == id, ct);

        public Task<bool> ExistsByIndexAsync(string indexNumber, CancellationToken ct = default)
            => _db.Students.AnyAsync(x => x.IndexNumber == indexNumber, ct);

        public Task<Student?> GetByIndexAsync(string indexNumber, CancellationToken ct = default)
          => _db.Students.FirstOrDefaultAsync(x => x.IndexNumber == indexNumber, ct);

        public Task<Student?> GetByIdWithUserAsync(int id, CancellationToken ct = default)
         => _db.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(x => x.ID == id, ct);
    }
}
