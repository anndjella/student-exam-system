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
        Task<RegistrationResponse> CreateAsync(int studentId, CreateRegistrationRequest req, CancellationToken ct = default);
        Task CancelAsync(int studentId, int subjectId, int termId, CancellationToken ct = default);
        Task<List<RegistrationResponse>> ListMyAsync(int studentId, CancellationToken ct = default);

    }
}
