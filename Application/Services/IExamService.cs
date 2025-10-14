using Application.DTO.Exams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface IExamService
    {
        Task<ExamResponse> CreateAsync(CreateExamRequest req, CancellationToken ct = default);
        Task<ExamResponse?> GetAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<ExamResponse>> ListAsync(CancellationToken ct = default);
        Task UpdateAsync(int id, UpdateExamRequest req, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
