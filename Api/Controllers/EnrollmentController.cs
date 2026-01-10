using Application.DTO.Enrollments;
using Application.DTO.Me.Student;
using Application.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    [Authorize(Roles = "StudentService", Policy = "PasswordChanged")]
    public sealed class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentService _svc;

        public EnrollmentsController(IEnrollmentService svc) => _svc = svc;

        [HttpPost("bulk-by-index-year")]
        public async Task<ActionResult<BulkEnrollResult>> BulkEnrollByIndexYear(
             [FromBody, CustomizeValidator(RuleSet = "Create")] BulkEnrollByIndexYearRequest req,
            CancellationToken ct)
        {
            var result = await _svc.BulkEnrollByIndexYearAsync(req, ct);
            return Ok(result);
        }
        [HttpPost("expire")]
        public async Task<ActionResult<int>> Expire(CancellationToken ct)
        {
            var updated = await _svc.ExpireActiveEnrollmentsAsync(ct);
            return Ok(updated);
        }

    }
}
