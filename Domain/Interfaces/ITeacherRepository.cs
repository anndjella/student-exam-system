using Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITeacherRepository
    {
        Task<bool> ExistsByJmbgAsync(string jmbg, CancellationToken ct = default);

        Task<Teacher> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<Teacher>> ListAsync(CancellationToken ct = default);
        Task<int> CreateAsync(Teacher teacher, CancellationToken ct = default);
        Task UpdateAsync(Teacher teacher, CancellationToken ct = default);
        Task DeleteAsync(Teacher teacher, CancellationToken ct = default);

    }
}
