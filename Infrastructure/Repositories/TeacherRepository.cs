using Application.DTO.Exams;
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
    public sealed class TeacherRepository : ITeacherRepository
    {
        private readonly AppDbContext _db;
        public TeacherRepository(AppDbContext db)
        {
            _db = db;
        }
        public void Add(Teacher teacher) => _db.Teachers.Add(teacher);
        public Task<bool> ExistsByEmployeeNumAsync(string employeeNum, CancellationToken ct = default)
          => _db.Teachers.AnyAsync(x => x.EmployeeNumber == employeeNum, ct);
        public Task<bool> ExistsByIdAsync(int id, CancellationToken ct = default)
          => _db.Teachers.AsNoTracking().AnyAsync(x => x.ID == id, ct);
        public Task DeleteByIdAsync(int teacherId, CancellationToken ct = default)
        => _db.Teachers.Where(e => e.ID == teacherId).ExecuteDeleteAsync();

        public  Task<Teacher?> GetByIdAsync(int id, CancellationToken ct = default)
           =>  _db.Teachers.FirstOrDefaultAsync(x => x.ID == id, ct);
       
        public Task<Teacher?> GetByIdWithUserAsync(int id, CancellationToken ct = default)
        =>_db.Teachers
            .Include(x=>x.User)
            .FirstOrDefaultAsync(x=>x.ID == id, ct);

        public Task<Teacher?> GetByEmployeeNumAsync(string employeeNum, CancellationToken ct = default)
        => _db.Teachers.FirstOrDefaultAsync(x => x.EmployeeNumber == employeeNum);
    }
}
