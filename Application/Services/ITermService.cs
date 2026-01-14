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
        Task<List<Term>> ListAsync(UserRole role, CancellationToken ct);
        Task<List<Term>> ListForGradingAsync(CancellationToken ct);      
        Task<List<Term>> ListOpenForRegistrationAsync(CancellationToken ct);

        Task UpdateAsync(int id, UpdateTermRequest req, CancellationToken ct = default);

        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
