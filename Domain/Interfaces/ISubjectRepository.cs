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
        //existence
        Task<bool> ExistsByCode(string code, CancellationToken ct);
        Task<bool> ExistsById(int id, CancellationToken ct);

        // read
        Task<Subject?> GetByIdWithTeachersAsync(int id, CancellationToken ct = default);
        Task<List<int>> GetExistingIdsAsync(List<int> ids, CancellationToken ct);
        Task<Subject?> GetByCodeWithTeachersAsync(string code, CancellationToken ct = default);
        Task<Subject?> GetByCodeAsync(string subjectCode, CancellationToken ct=default);
        Task<List<Subject>> ListActiveAsync(CancellationToken ct = default);
        //Task<List<Subject>> ListAllWithTeachersAsync(CancellationToken ct=default);
        Task<int> CountAdminAsync(bool isActive, string? query, CancellationToken ct = default);
        Task<List<Subject>> ListPagedWithTeachersAsync(int skip, int take, bool isActive, string? query, CancellationToken ct = default);

        // write
        void Add(Subject subject);

        void Remove(Subject subject);
    }
}
