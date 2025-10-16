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

        public async Task<int> CreateAsync(Teacher teacher, CancellationToken ct = default)
        {
            _db.Teachers.Add(teacher);
            await _db.SaveChangesAsync(ct);
            return teacher.ID;
        }

        public async Task DeleteAsync(Teacher teacher, CancellationToken ct = default)
        {
            _db.Teachers.Remove(teacher);
            await _db.SaveChangesAsync(ct);
        }

        public Task<bool> ExistsByJmbgAsync(string jmbg, CancellationToken ct = default)
        {
            return _db.Teachers.AnyAsync(x => x.JMBG == jmbg, ct);
        }

        public async Task<Teacher?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Teachers.FirstOrDefaultAsync(x => x.ID == id, ct);
        }

        public async Task<IReadOnlyList<Teacher>> ListAsync(CancellationToken ct = default)
        {
          return await _db.Teachers.AsNoTracking().OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync(ct);

        }

        public async Task<IReadOnlyList<Exam>> ListExamsAsExaminerAsync(int teacherId, CancellationToken ct = default)
        {
            return await _db.Exams.AsNoTracking()
                       .Where(e => e.ExaminerID == teacherId)
                       .Include(e=>e.Student)
                       .Include(e => e.Subject)
                       .Include(e => e.Examiner)
                       .Include(e => e.Supervisor)
                       .OrderByDescending(e => e.Date).ThenByDescending(e => e.ID)
                       .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Exam>> ListExamsAsSupervisorAsync(int teacherId, CancellationToken ct = default)
        {
            return await _db.Exams.AsNoTracking()
                 .Where(e => e.SupervisorID == teacherId)
                 .Include(e => e.Student)
                 .Include(e => e.Subject)
                 .Include(e => e.Examiner)
                 .Include(e => e.Supervisor)
                 .OrderByDescending(e => e.Date).ThenByDescending(e => e.ID)
                 .ToListAsync(ct);
        }

        public async Task UpdateAsync(Teacher teacher, CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}
