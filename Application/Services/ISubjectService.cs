using Application.DTO.Me.Teacher;
using Application.DTO.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface ISubjectService
    {
        Task<SubjectResponse> CreateAsync(CreateSubjectRequest req, CancellationToken ct = default);
        Task<AdminSubjectResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<AdminSubjectResponse?> GetByCodeAsync(string code, CancellationToken ct = default);
        Task<AdminSubjectsResponse> ListAllWithTeachersAsync(CancellationToken ct);
        Task<List<SubjectResponse>> ListActiveAsync(CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        Task DeactivateAsync(int id, CancellationToken ct);
    }
}
