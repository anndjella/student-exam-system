using Application.DTO.Me;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface IMeService
    {
        Task<MeResponse> GetMeAsync(int personId, CancellationToken ct = default);

    }
}
