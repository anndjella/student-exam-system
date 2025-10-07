using Application.DTO.Students;

namespace Application.Services
{
    public interface IStudentService
    {
        Task<StudentResponse> CreateAsync(CreateStudentRequest req, CancellationToken ct = default);
        Task<StudentResponse?> GetAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<StudentResponse>> ListAsync(CancellationToken ct = default);
        Task UpdateAsync(int id, UpdateStudentRequest req, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
