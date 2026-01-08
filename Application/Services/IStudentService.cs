using Application.DTO.Exams;
using Application.DTO.Students;

namespace Application.Services
{
    public interface IStudentService
    {
        Task<StudentResponse> CreateAsync(CreateStudentRequest req, CancellationToken ct = default);
        Task<StudentResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<StudentResponse?> GetByIndexAsync(string index, CancellationToken ct = default);
        Task UpdateAsync(int id, UpdateStudentRequest req, CancellationToken ct = default);
        Task SoftDeleteAsync(int id, CancellationToken ct = default);
    }
}
