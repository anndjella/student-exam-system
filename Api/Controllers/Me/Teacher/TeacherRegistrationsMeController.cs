using Api.Common;
using Application.DTO.Me.Teacher;
using Application.DTO.Registrations;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers.Me
{
    [ApiController]
    [Route("api/me/teacher/registrations")]
    [Authorize(Roles = "Teacher", Policy = "PasswordChanged")]
    public sealed class TeacherRegistrationsMeController : ControllerBase
    {
        private readonly IRegistrationService _svc;

        public TeacherRegistrationsMeController(IRegistrationService svc)
            => _svc = svc;

        [HttpGet("subject/{subjectId:int}/term/{termId:int}")]
        public async Task<ActionResult<List<TeacherRegistrationResponse>>> ListForSubjectAndTerm(
            int subjectId, int termId, CancellationToken ct)
        {
            if (!User.TryGetPid(out var personId)) return Unauthorized();

            var res = await _svc.ListMyActiveBySubjectAndTermAsync(personId, subjectId, termId, ct);
            return Ok(res);
        }
    }
}
