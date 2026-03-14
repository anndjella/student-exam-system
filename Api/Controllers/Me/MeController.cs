using Api.Common;
using Application.DTO.Me;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Me
{
    [ApiController]
    [Route("api/me")]
    [Authorize(Policy = "PasswordChanged")]
    public class MeController : ControllerBase
    {
        private readonly IMeService _svc;
        public MeController(IMeService svc) => _svc = svc;

        [HttpGet]
        public async Task<ActionResult<MeResponse>> Get(CancellationToken ct)
        {
            if (!User.TryGetPid(out var personId)) return Unauthorized();

            var resp = await _svc.GetMeAsync(personId, ct);
            return Ok(resp);
        }
    }
}
