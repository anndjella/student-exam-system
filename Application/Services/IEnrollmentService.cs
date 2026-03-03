using Application.DTO.Common;
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
        //Task<PagedResponse<EnrollmentResponse>> ListAsync(int skip, int take, string? query, CancellationToken ct);
        Task<PagedResponse<EnrollmentResponse>> ListByStudentAsync(string index,int skip, int take, string? query, CancellationToken ct);
        Task<PagedResponse<EnrollmentResponse>> ListBySubjectAsync(string code,int skip, int take, string? query, CancellationToken ct);

        Task DeleteAsync(int studentId, int subjectId, CancellationToken ct = default);
        Task<EnrollmentResponse> CreateAsync(CreateEnrollmentRequest req, CancellationToken ct = default);

    }
}
