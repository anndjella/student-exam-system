using Domain.Entity;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IRegistrationRepository
    {
        Task<Registration?> GetAsync(int studentId, int subjectId, int termId, CancellationToken ct = default);
        Task<bool> ExistsActiveAsync(int studentId, int subjectId, int termId, CancellationToken ct = default);
        Task<List<Registration>> ListActiveForStudentAsync(int studentId, CancellationToken ct = default);
        Task<List<int>> ListActiveStudentIdsAsync(int subjectId, int termId, CancellationToken ct = default);
        Task<List<Registration>> ListActiveBySubjectAndTermWithExamAsync(int subjectId, int termId, CancellationToken ct = default);
        Task<bool> ExistsAnyForSubjectAsync(int subjectId, CancellationToken ct);

        Task<bool> ExistsAnyForTermAsync(int termId, CancellationToken ct = default);

        void Add(Registration r);
    }
}
