using Application.DTO.Term;
using Domain.Entity;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface ITermService
    {
        Task<TermResponse> CreateAsync(CreateTermRequest req, CancellationToken ct = default);
        Task<TermResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<TermResponse>> ListAsync(UserRole role, CancellationToken ct);
        Task<List<TermResponse>> ListForGradingAsync(int teacherId, int subjectId, CancellationToken ct);      
        Task<List<TermResponse>> ListOpenForRegistrationAsync(CancellationToken ct);
        Task<List<TermResponse>> ListForTeacherExamsViewAsync(CancellationToken ct);

        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
