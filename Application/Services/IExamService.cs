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
        Task<TeacherExamItemResponse> CreateAsync(int subjectId, int termId, int studentId,CreateExamRequest req, int teacherId, CancellationToken ct = default);
        Task<TeacherExamItemResponse> UpdateAsync(int examId,UpdateExamRequest req, int teacherId, CancellationToken ct = default);
        Task<int> LockAsync(LockExamsRequest req, int teacherId, CancellationToken ct = default);
        Task<StudentExamsResponse> ListMySignedAsync(int studentId, CancellationToken ct = default);
        Task<TeacherExamsResponse> ListBySubjectTermAsync(int subjectId, int termId, int teacherId, CancellationToken ct = default);
        Task<StudServiceExamsResponse> ListPagedAsync(int subjectId, int termId, int skip, int take, string? query, CancellationToken ct = default);
    }
}
