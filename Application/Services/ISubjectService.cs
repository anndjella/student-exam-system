using Application.DTO.Common;
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
        Task<StudServiceSubjectResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<StudServiceSubjectResponse?> GetByCodeAsync(string code, CancellationToken ct = default);
        //Task<AdminSubjectsResponse> ListAllWithTeachersAsync(CancellationToken ct);
        Task<PagedResponse<StudServiceSubjectResponse>> ListPagedAsync(bool isActive, int skip, int take, string? query, CancellationToken ct);
        Task<List<SimpleSubjectResponse>> ListAllIncludingInactiveAsync(CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        Task DeactivateAsync(int id, CancellationToken ct);
    }
}
