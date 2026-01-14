using Application.DTO.Enrollments;
using Application.DTO.Me.Student;
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

        public Task<MyEnrolledSubjectsResponse> GetMyEnrolledSubjectsAsync(int personId, CancellationToken ct);
    }
}
