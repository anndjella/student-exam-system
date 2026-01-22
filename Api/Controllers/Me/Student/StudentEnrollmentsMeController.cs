using Application.Services;
using Api.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers.Me.Student
{
    [ApiController]
    [Route("api/me/student/subjects")]
    [Authorize(Roles = "Student", Policy = "PasswordChanged")]
    public class StudentEnrollmentsMeController : ControllerBase
    {
        private readonly IEnrollmentService _svc;
        public StudentEnrollmentsMeController(IEnrollmentService svc)
        {
            _svc = svc;
        }
        [HttpGet("my-enrolled-subjects")]
        public async Task<IActionResult> GetMySubjects(CancellationToken ct)
        {
            if (!User.TryGetPid(out var studentId))
                return Unauthorized();

            return Ok(await _svc.ListStudentSubjectsAsync(studentId, ct));
        }
        [HttpGet("not-passed-subjects")]
        public async Task<IActionResult> GetMyNotPassed(CancellationToken ct)
        {
            if (!User.TryGetPid(out var studentId))
                return Unauthorized();

            return Ok(await _svc.ListNotPassedAsync(studentId, ct));
        }
    }
}
