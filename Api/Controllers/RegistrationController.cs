using Application.DTO.Common;
using Application.DTO.Enrollments;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Authorize(Roles ="StudentService",Policy ="PasswordChanged")]
    [Route("api/registrations")]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationService _svc;

        public RegistrationController(IRegistrationService svc) => _svc = svc;

        [HttpGet]
        public async Task<ActionResult<PagedResponse<EnrollmentResponse>>> List(
            [FromQuery] int subjectId,
            [FromQuery] int termId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 20,
            [FromQuery] string? query = null,
            CancellationToken ct = default)
        {
            if (skip < 0) skip = 0;
            if (take <= 0) take = 20;
            if (take > 100) take = 100;

            var res = await _svc.ListPagedAsync(subjectId, termId, skip, take, query, ct);
            return Ok(res);
        }
    }
}
