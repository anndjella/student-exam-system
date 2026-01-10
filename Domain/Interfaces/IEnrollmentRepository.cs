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
            int schoolYearId,
            List<int> subjectIds,
            CancellationToken ct);

        public void AddRange(IEnumerable<Enrollment> enrollments);
        Task<List<Enrollment>> ListByStudentIdAsync(int studentId, CancellationToken ct);
        Task<List<Enrollment>> ListActiveAsync(DateOnly today, CancellationToken ct);
    }
}
