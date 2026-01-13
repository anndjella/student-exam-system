using Application.DTO.Me.Teacher;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers.Me
{
    [ApiController]
    [Route("api/me")]
    [Authorize(Roles = "Teacher", Policy = "PasswordChanged")]
    public class TeachersMeController : ControllerBase
    {
        private readonly ITeachingAssignmentService _svc;
        public TeachersMeController(ITeachingAssignmentService svc) { _svc = svc; }
        [HttpGet("teaching-assignments")]
        public async Task<ActionResult<MyTeachingAssignmentsResponse>> GetMyTeachingAssignments( CancellationToken ct)
        {
            var pidStr = User.FindFirstValue("pid");
            if (!int.TryParse(pidStr, out var personId))
                return Unauthorized();

            return Ok(await _svc.GetMyTeachingAssignments(personId, ct));

        }

    }
}
