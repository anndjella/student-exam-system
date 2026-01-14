using Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITermRepository
    {
        Task<Term?> GetByIdAsync(int termId,CancellationToken ct);
        Task<bool> ExistsOverlapAsync(DateOnly start, DateOnly end, int? excludeId, CancellationToken ct);
        Task<List<Term>> ListAllAsync(CancellationToken ct);
        Task<List<Term>> ListOpenForRegistrationAsync(DateOnly today, CancellationToken ct);
        Task<List<Term>> ListCurrentAndFutureAsync(DateOnly today, CancellationToken ct);
        Task<List<Term>> ListCurrentAndPrev2Async(DateOnly today, CancellationToken ct);

        void Add(Term term); 
        void Remove(Term term);
    }
}
