using Api.Common;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers.Me
{
    [ApiController]
    [Route("api/me/teacher/subjects")]
    [Authorize(Roles = "Teacher", Policy = "PasswordChanged")]
    public sealed class TeacherSubjectsMeController : ControllerBase
    {
        private readonly ITeachingAssignmentService _svc;

        public TeacherSubjectsMeController(ITeachingAssignmentService svc)
            => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> ListMySubjects(CancellationToken ct)
        {
            if (!User.TryGetPid(out var personId)) return Unauthorized();

            var res = await _svc.ListTeacherSubjectsDividedAsync(personId, ct);
            return Ok(res);
        }
    }
}
