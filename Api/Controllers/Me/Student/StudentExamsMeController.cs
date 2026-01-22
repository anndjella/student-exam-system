using Api.Common;
using Application.DTO.Me.Student;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Me.Student
{
    [ApiController]
    [Route("api/me/student/exams")]
    [Authorize(Roles = "Student", Policy = "PasswordChanged")]
    public class StudentExamsMeController : ControllerBase
    {
        private readonly IExamService _svc;
        public StudentExamsMeController(IExamService svc) { _svc = svc; }
        [HttpGet("signed")]
        public async Task<ActionResult<StudentExamsResponse>> GetSigned(CancellationToken ct)
        {
            if (!User.TryGetPid(out var personId))
                return Unauthorized();

            var res = await _svc.ListMySignedAsync(personId, ct);
            return Ok(res);
        }
    }
}
