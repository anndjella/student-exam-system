using Domain.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ISubjectRepository
    {
        // read
        Task<Subject?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Subject?> GetByNameAsync(string name, CancellationToken ct = default);
        public Task<List<int>> GetExistingIdsAsync(List<int> ids, CancellationToken ct);

        // write
        void Add(Subject subject);
    }
}
