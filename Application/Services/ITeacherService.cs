using Application.DTO.Common;
using Application.DTO.Exams;
using Application.DTO.Teachers;

namespace Application.Services
{
    public interface ITeacherService
    {
        Task<TeacherResponse> CreateAsync(CreateTeacherRequest req,CancellationToken ct=default);
        Task<TeacherResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<TeacherResponse?> GetByNumAsync(string employeeNum, CancellationToken ct = default);
        Task<PagedResponse<TeacherResponse>> ListAsync(int skip, int take, string? query, CancellationToken ct);
        Task UpdateAsync(int id, UpdateTeacherRequest req, CancellationToken ct = default);
        Task SoftDeleteAsync(int id, CancellationToken ct = default);
    }
}
