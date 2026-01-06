using Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUserRepository
    {
        //// existence
        //Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);

        //// read
        //Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
        //Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
        //Task<User?> GetByPersonIdAsync(int personId, CancellationToken ct = default);

        //// write
        //Task<int> CreateAsync(User user, CancellationToken ct = default);
        //Task UpdateAsync(User user, CancellationToken ct = default);

        //existance
        Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);

        // read
        Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
        Task<User?> GetByPersonIdAsync(int personId, CancellationToken ct = default);

        // write (bez SaveChanges)
        void Add(User user);
        void Update(User user);
    }
}
