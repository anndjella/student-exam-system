using Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public  interface IStudentRepository
    {
        // existence checks
        Task<bool> ExistsByIndexAsync(string indexNumber, CancellationToken ct = default);

        // read
        Task<Student?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Student?> GetByIdWithUserAsync(int id, CancellationToken ct = default);
        Task<Student?> GetByIndexAsync(string indexNumber, CancellationToken ct = default);
        public Task<List<int>> ListIdsByIndexPrefixAsync(string prefix, CancellationToken ct);
        Task<int> CountAsync(string? query, CancellationToken ct = default);
        Task<List<Student>> ListPagedAsync(int skip, int take, string? query, CancellationToken ct = default);

        // write
        void Add(Student student);
    }
}
