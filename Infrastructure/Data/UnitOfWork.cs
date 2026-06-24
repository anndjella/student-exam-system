using Application.Common;
using Domain.Interfaces;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _db;

        public UnitOfWork(AppDbContext db)
        {
            _db = db;
            Students = new StudentRepository(_db);
            Teachers = new TeacherRepository(_db);
            People = new PersonRepository(_db);
            Users = new UserRepository(_db);
            Subjects = new SubjectRepository(_db);
            Enrollments = new EnrollmentRepository(_db);
            TeachingAssignments = new TeachingAssignmentRepository(_db);
            Terms = new TermRepository(_db);
            Registrations = new RegistrationRepository(_db);
            Exams = new ExamRepository(_db);
            StudentStats = new StudentStatsRepository(_db);
        }

        public IStudentRepository Students { get; }
        public ITeacherRepository Teachers { get; }
        public IPersonRepository People { get; }
        public IUserRepository Users { get; }
        public ISubjectRepository Subjects { get; }
        public IEnrollmentRepository Enrollments { get; }
        public ITeachingAssignmentRepository TeachingAssignments { get; }
        public ITermRepository Terms { get; }
        public IRegistrationRepository Registrations { get; }
        public IExamRepository Exams { get; }
        public IStudentStatsRepository StudentStats { get; }

        public Task<int> CommitAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
