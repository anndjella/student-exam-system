using Application.Common;
using Domain.Interfaces;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _db;

        public UnitOfWork(AppDbContext db)
        {
            _db = db;    
        }
        public IStudentRepository Students => new StudentRepository(_db);
        public ITeacherRepository Teachers => new TeacherRepository(_db);
        public IPersonRepository People => new PersonRepository(_db);
        public IUserRepository Users => new UserRepository(_db);
        public ISubjectRepository Subjects => new SubjectRepository(_db);
        public ISchoolYearRepository SchoolYears => new SchoolYearRepository(_db);
        public IEnrollmentRepository Enrollments => new EnrollmentRepository(_db);

        public Task<int> CommitAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
