using Application.DTO.Me.Student;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers.Me
{
    [ApiController]
    [Route("api/me")]
    [Authorize(Roles = "Student", Policy = "PasswordChanged")]
    public sealed class StudentsMeController : ControllerBase
    {
        private readonly IEnrollmentService _svc;
        public StudentsMeController(IEnrollmentService svc) => _svc = svc;

        [HttpGet("subjects")]
        public async Task<ActionResult<MyEnrolledSubjectsResponse>> GetMySubjects(CancellationToken ct)
        {
            var pidStr = User.FindFirstValue("pid");
            if (!int.TryParse(pidStr, out var personId))
                return Unauthorized();

            return Ok(await _svc.GetMySubjectsAsync(personId, ct));
        }
    }
}
