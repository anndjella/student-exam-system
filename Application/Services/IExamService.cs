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
        Task CreateAsync(CreateExamRequest req, CancellationToken ct = default);
       // Task<ExamResponse?> GetAsync(int studentId, int subjectId, DateOnly date, CancellationToken ct = default);
       // Task<IReadOnlyList<ExamResponse>> ListAsync(CancellationToken ct = default);
        Task UpdateAsync(int studentId, int subjectId, DateOnly date, UpdateExamRequest req, CancellationToken ct = default);
        Task DeleteAsync(int studentId, int subjectId, DateOnly date, CancellationToken ct = default);
    }
}
