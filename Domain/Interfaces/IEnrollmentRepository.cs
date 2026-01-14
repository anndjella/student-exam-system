using Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IEnrollmentRepository
    {
        public Task<HashSet<(int StudentId, int SubjectId)>> ListExistingPairsAsync(
            List<int> studentIds,
            List<int> subjectIds,
            CancellationToken ct);

        public void AddRange(IEnumerable<Enrollment> enrollments);
        Task<List<Enrollment>> ListByStudentIdAsync(int studentId, CancellationToken ct);
        Task<bool> ExistsAsync(int studentId, int subjectId, CancellationToken ct);
        Task<bool> IsPassedAsync(int studentId, int subjectId, CancellationToken ct);
        Task<Enrollment?> GetAsync(int studentId, int subjectId, CancellationToken ct);
    }
}
