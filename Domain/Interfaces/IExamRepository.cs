using Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IExamRepository
    {
        Task<bool> ExistsOnDateAsync(int studentId, int subjectId, DateOnly date, CancellationToken ct = default);
        Task<bool> HasPassedAsync(int studentId, int subjectId, CancellationToken ct = default);
        Task<Exam?> GetByKeyAsync(int studentId, int subjectId, DateOnly date, CancellationToken ct = default);
        Task<Exam?> GetByKeyWithDetailsAsync(int studentId, int subjectId, DateOnly date, CancellationToken ct = default);
        Task<IReadOnlyList<Exam>> ListWithDetailsAsync(CancellationToken ct = default);
        Task CreateAsync(Exam exam, CancellationToken ct = default);
        Task UpdateAsync(Exam exam, CancellationToken ct = default);
        Task DeleteAsync(int studentId, int subjectId, DateOnly date, CancellationToken ct = default);
    }
}
