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
        Task<List<Enrollment>> ListByStudentIdWithSubjectAndTeachersAsync(int studentId, CancellationToken ct);
        Task<List<Enrollment>> ListNotPassed(int studentId, CancellationToken ct);
        Task<List<Enrollment>> ListByStudentsAndSubjectAsync(List<int> studentIds,int subjectId, CancellationToken ct);
        Task<bool> ExistsAsync(int studentId, int subjectId, CancellationToken ct);
        Task<bool> ExistsBySubjectIdAsync(int subjectId, CancellationToken ct);
        Task<bool> IsPassedAsync(int studentId, int subjectId, CancellationToken ct);
        Task<int> CountByStudentAsync(int studentId, string? query, CancellationToken ct = default);
        Task<int> CountBySubjectAsync(int subjectId, string? query, CancellationToken ct = default);

        Task<List<Enrollment>> ListPagedAsync(int skip, int take, string? query, CancellationToken ct = default);
        Task<List<Enrollment>> ListPagedBySubjectAsync(int subjectId,int skip, int take, string? query, CancellationToken ct = default);
        Task<List<Enrollment>> ListPagedByStudentAsync(int studentId, int skip, int take, string? query, CancellationToken ct = default);

        Task<Enrollment?> GetAsync(int studentId, int subjectId, CancellationToken ct);
        void Remove(Enrollment enrollment);
        void Add(Enrollment enrollment);

    }
}
