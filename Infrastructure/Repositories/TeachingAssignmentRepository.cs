using Application.Services;
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
    public sealed class TeachingAssignmentRepository : ITeachingAssignmentRepository
    {
        private readonly AppDbContext _db;
        public TeachingAssignmentRepository(AppDbContext db) { _db = db; }
        public void Add(TeachingAssignment teachingAssignment)
         => _db.TeachingAssignments.Add(teachingAssignment);
        public Task<bool> ExistsAsync(int teacherId, int subjectId, CancellationToken ct)
          => _db.TeachingAssignments.AsNoTracking().AnyAsync(e => e.SubjectID == subjectId && e.TeacherID == teacherId, ct);

        public Task<TeachingAssignment?> GetAsync(int teacherId, int subjectId, CancellationToken ct)
         => _db.TeachingAssignments
            .Include(e => e.Teacher)
            .Include(e => e.Subject)
            .FirstOrDefaultAsync(e => e.SubjectID == subjectId && e.TeacherID == teacherId, ct); 

        public Task<List<TeachingAssignment>> ListBySubjectIdAsync(int subjectId, CancellationToken ct)
        =>_db.TeachingAssignments.Where(e=>e.SubjectID==subjectId)
            .Include(e=>e.Teacher)
            .Include(e=>e.Subject)
            .ToListAsync(ct);

        public Task<List<TeachingAssignment>> ListByTeacherIdAsync(int teacherId, CancellationToken ct)
        => _db.TeachingAssignments.Where(e => e.TeacherID == teacherId)
            .Include(e=>e.Subject)
            .ThenInclude(e=>e.TeachingAssignments)
            .Include(e=>e.Teacher)
            .ToListAsync(ct);

        public Task<List<TeachingAssignment>> ListByTeacherIdWithSubjectAndTeachersAsync(int teacherId, CancellationToken ct)
        => _db.TeachingAssignments.Where(e => e.TeacherID == teacherId)
            .Include(e => e.Subject)
            .ThenInclude(e => e.TeachingAssignments)
            .ThenInclude(e => e.Teacher).ToListAsync(ct);

        public void Remove(TeachingAssignment assignment)
        =>_db.TeachingAssignments.Remove(assignment);
        //public Task<bool> CanTeacherGradeAsync(int teacherId, int subjectId, CancellationToken ct)
        //=>_db.TeachingAssignments.AsNoTracking().AnyAsync(
        //    e => e.SubjectID == subjectId
        //    && e.TeacherID == teacherId 
        //    && e.CanGrade, ct);
    }
}
