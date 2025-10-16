using Application.DTO.Exams;
using Application.DTO.Teachers;

namespace Application.Services
{
    public interface ITeacherService
    {
        Task<TeacherResponse> CreateAsync(CreateTeacherRequest req,CancellationToken ct=default);
        Task<TeacherResponse?> GetAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<TeacherResponse>> ListAsync(CancellationToken ct = default);

        Task UpdateAsync(int id, UpdateTeacherRequest req, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<ExamResponse>> ListExamsAsExaminerAsync(int teacherId, CancellationToken ct = default);
        Task<IReadOnlyList<ExamResponse>> ListExamsAsSupervisorAsync(int teacherId, CancellationToken ct = default);
    }
}
