using Api.Auth;
using Application.DTO.Me.Student;
using Application.DTO.Registrations;
using Application.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers.Me.Students
{
    [ApiController]
    [Route("api/me/registrations")]
    [Authorize(Roles = "Student", Policy = "PasswordChanged")]
    public sealed class MyRegistrationsController : ControllerBase
    {
        private readonly IRegistrationService _svc;
        public MyRegistrationsController(IRegistrationService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody, CustomizeValidator(RuleSet = "Create")] CreateRegistrationRequest req,
            CancellationToken ct)
        {
            var pidStr = User.FindFirstValue("pid");
            if (!int.TryParse(pidStr, out var personId))
                return Unauthorized();
            var resp = await _svc.CreateAsync(personId, req, ct);
            return Ok(resp);
        }

        [HttpPut("cancel/{subjectId:int}/{termId:int}")]
        public async Task<IActionResult> Cancel(int subjectId, int termId, CancellationToken ct)
        {
            var pidStr = User.FindFirstValue("pid");
            if (!int.TryParse(pidStr, out var personId))
                return Unauthorized();
            await _svc.CancelAsync(personId, subjectId, termId, ct);
            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<List<RegistrationResponse>>> List(CancellationToken ct)
        {
            var pidStr = User.FindFirstValue("pid");
            if (!int.TryParse(pidStr, out var personId))
                return Unauthorized();
            var list = await _svc.ListMyAsync(personId, ct);
            return Ok(list);
        }
    }
}
