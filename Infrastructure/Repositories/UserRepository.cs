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
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default)
            => _db.Users.AnyAsync(u => u.Username == username, ct);

        public Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
            => _db.Users.FirstOrDefaultAsync(u => u.ID == id, ct);

        public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
            => _db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

        public Task<User?> GetByPersonIdAsync(int personId, CancellationToken ct = default)
            => _db.Users.FirstOrDefaultAsync(u => u.PersonID == personId, ct);

        public void Add(User user)
            => _db.Users.Add(user);

        public void Update(User user)
            => _db.Users.Update(user);

    }
}
