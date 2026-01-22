using Application.DTO.Exams;
using Application.DTO.Me.Student;
using Application.DTO.Me.StudService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface IExamService
    {
        Task<ExamResponse> CreateAsync(int subjectId, int termId, int studentId,CreateExamRequest req, int teacherId, CancellationToken ct = default);
        Task<ExamResponse> UpdateAsync(int subjectId, int termId, int studentId,UpdateExamRequest req, int teacherId, CancellationToken ct = default);
        Task<int> LockAsync(LockExamsRequest req, int teacherId, CancellationToken ct = default);
        Task<StudentExamsResponse> ListMySignedAsync(int studentId, CancellationToken ct = default);
        Task<StudentServiceExamsResponse> ListAllBySubjectTermAsync(int subjectId, int termId, CancellationToken ct = default);

        Task<List<ExamResponse>> ListBySubjectTermAsync(int subjectId, int termId, int teacherId, CancellationToken ct = default);

    }
}
