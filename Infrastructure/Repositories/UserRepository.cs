using Domain.Entity;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext db) : base(db) { }

        public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default)
            => Set.AnyAsync(u => u.Username == username, ct);

        public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
            => Set.FirstOrDefaultAsync(u => u.Username == username, ct);

        public Task<User?> GetByPersonIdAsync(int personId, CancellationToken ct = default)
            => Set.FirstOrDefaultAsync(u => u.PersonID == personId, ct);

    }
}
