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
        Task<SubjectResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<SubjectResponse?> GetByNameAsync(string name, CancellationToken ct = default);
        Task UpdateAsync(int id, UpdateSubjectRequest req, CancellationToken ct = default);
        Task SoftDeleteAsync(int id, CancellationToken ct = default);
    }
}
