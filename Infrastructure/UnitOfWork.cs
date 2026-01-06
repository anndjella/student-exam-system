using Application.Common;
using Domain.Interfaces;
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

        public UnitOfWork(
            AppDbContext db,
            IStudentRepository students,
            //ITeacherRepository teachers,
            IPersonRepository people,
            IUserRepository users)
        {
            _db = db;
            Students = students;
            //Teachers = teachers;
            People = people;
            Users = users;
        }

        public IStudentRepository Students { get; }
        //public ITeacherRepository Teachers { get; }
        public IPersonRepository People { get; }
        public IUserRepository Users { get; }

        public Task<int> CommitAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
