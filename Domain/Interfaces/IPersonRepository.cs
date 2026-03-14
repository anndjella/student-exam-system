using Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IPersonRepository
    {
        Task<bool> ExistsByJmbgAsync(string jmbg, CancellationToken ct = default);
        Task<Person?> GetByIdAsync(int personId, CancellationToken ct = default);
    }
}
