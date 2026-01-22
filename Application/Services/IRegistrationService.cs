using Application.DTO.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface IRegistrationService
    {
        Task<StudentRegistrationResponse> CreateAsync(int studentId, CreateRegistrationRequest req, CancellationToken ct = default);
        Task CancelAsync(int studentId, int subjectId, int termId, CancellationToken ct = default);
        Task<List<StudentRegistrationResponse>> ListMyActiveAsync(int studentId, CancellationToken ct = default);
        Task<List<TeacherRegistrationResponse>> ListMyActiveBySubjectAndTermAsync(int teacherId, int subjectId, int termId, CancellationToken ct = default); 
    }
}
