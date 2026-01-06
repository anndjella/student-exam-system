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
        public Task<bool> ExistsByEmployeeNumAsync(string employeeNum, CancellationToken ct = default)
          => _db.Teachers.AsNoTracking().AnyAsync(x => x.EmployeeNumber == employeeNum, ct);

        public async Task<int> CreateAsync(Teacher teacher, CancellationToken ct = default)
        {
            _db.Teachers.Add(teacher);
            await _db.SaveChangesAsync(ct);
            return teacher.ID;
        }

        public Task DeleteByIdAsync(int teacherId, CancellationToken ct = default)
        => _db.Teachers.Where(e => e.ID == teacherId).ExecuteDeleteAsync();


        public  Task<Teacher?> GetByIdAsync(int id, CancellationToken ct = default)
           =>  _db.Teachers.FirstOrDefaultAsync(x => x.ID == id, ct);
        

        public Task UpdateAsync(Teacher teacher, CancellationToken ct = default) 
           =>  _db.SaveChangesAsync(ct);
        
    }
}
