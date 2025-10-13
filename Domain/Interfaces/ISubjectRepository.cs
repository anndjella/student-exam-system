using Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ISubjectRepository
    {
        // read
        Task<Subject?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<Subject>> ListAsync(CancellationToken ct = default);

        // write
        Task<int> CreateAsync(Subject subject, CancellationToken ct = default);
        Task UpdateAsync(Subject subject, CancellationToken ct = default);
        Task DeleteAsync(Subject subject, CancellationToken ct = default);
    }
}
