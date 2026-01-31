using Api.Common;
using Application.DTO.Registrations;
using Application.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Me.Student
{
    [ApiController]
    [Route("api/me/student/registrations")]
    [Authorize(Roles = "Student", Policy = "PasswordChanged")]
    public class StudentRegistrationsMeController : ControllerBase
    {
        private readonly IRegistrationService _svc;
        public StudentRegistrationsMeController(IRegistrationService svc)
        {
            _svc = svc;
        }
        [HttpPost("registrations")]
        public async Task<IActionResult> Create(
          [FromBody, CustomizeValidator(RuleSet = "Create")] CreateRegistrationRequest req,
          CancellationToken ct)
        {
            if (!User.TryGetPid(out var studentId))
                return Unauthorized();

            var resp = await _svc.CreateAsync(studentId, req, ct);
            return Ok(resp);
        }

        [HttpPut("cancel/subject/{subjectId:int}/term/{termId:int}")]
        public async Task<IActionResult> Cancel(int subjectId, int termId, CancellationToken ct)
        {
            if (!User.TryGetPid(out var studentId))
                return Unauthorized();

            await _svc.CancelAsync(studentId, subjectId, termId, ct);
            return NoContent();
        }

        [HttpGet("my-active-registrations")]
        public async Task<ActionResult<List<StudentRegistrationResponse>>> List(CancellationToken ct)
        {
            if (!User.TryGetPid(out var studentId))
                return Unauthorized();

            var list = await _svc.ListMyActiveAsync(studentId, ct);
            return Ok(list);
        }
    }
}
