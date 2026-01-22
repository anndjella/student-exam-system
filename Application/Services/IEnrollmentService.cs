using Application.DTO.Enrollments;
using Application.DTO.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface IEnrollmentService
    {
        Task<BulkEnrollResult> BulkEnrollByIndexYearAsync(
            BulkEnrollByIndexYearRequest req,
            CancellationToken ct);
        Task<List<SubjectResponse>> ListStudentSubjectsAsync(int studentId, CancellationToken ct = default);
        Task<List<SubjectResponse>> ListNotPassedAsync(int studentId, CancellationToken ct);
    }
}
