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
        Task<ExamResponse> UpsertAsync(UpsertExamRequest req, int teacherId, CancellationToken ct = default);
        Task<int> LockAsync(LockExamsRequest req, int teacherId, CancellationToken ct = default);

        Task<List<ExamResponse>> ListBySubjectTermAsync(int subjectId, int termId, int teacherId, CancellationToken ct = default);

    }
}
