using Api.Common;
using Application.DTO.Term;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.Pkcs;

namespace Api.Controllers.Me.Teacher
{
    [ApiController]
    [Route("api/me/teacher/terms")]
    [Authorize(Roles = "Teacher", Policy = "PasswordChanged")]
    public class TeacherTermsMeController : ControllerBase
    {
        private readonly ITermService _svc;

        public TeacherTermsMeController(ITermService svc)
            => _svc = svc;

        [HttpGet("for-grading/subject/{subjectId:int}")]
        public async Task<ActionResult<List<TermResponse>>> ListForGrading(int subjectId,CancellationToken ct)
        {
            if (!User.TryGetPid(out var personId)) return Unauthorized();
            var resp = await _svc.ListForGradingAsync(personId, subjectId, ct);
            return resp is null ? NotFound() : Ok(resp);
        }
    }
}
