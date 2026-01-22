using Api.Common;
using Application.DTO.Exams;
using Application.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Me.Teacher
{
    [ApiController]
    [Route("api/me/teacher/subjects")]
    [Authorize(Roles = "Teacher", Policy = "PasswordChanged")]
    public class TeacherExamsMeController : ControllerBase
    {
        private readonly IExamService _svc;

        public TeacherExamsMeController(IExamService svc)
            => _svc = svc;

        [HttpPost("{subjectId:int}/terms/{termId:int}/students/{studentId:int}")]
        public async Task<ActionResult<ExamResponse>> Create(
             int subjectId,
             int termId,
             int studentId,
                [FromBody] CreateExamRequest req,
                CancellationToken ct)
        {
            if (!User.TryGetPid(out var teacherId))
                return Unauthorized();

            var res = await _svc.CreateAsync(subjectId, termId, studentId, req, teacherId, ct);
            return Ok(res);
        }
        [HttpPost("lock")]
        public async Task<IActionResult> Lock(
           [FromBody] LockExamsRequest req,
           CancellationToken ct)
        {
            var teacherId = int.Parse(User.FindFirst("pid")!.Value);
            var lockedCount = await _svc.LockAsync(req, teacherId, ct);
            return Ok(lockedCount);
        }
        [HttpPatch("{subjectId:int}/terms/{termId:int}/students/{studentId:int}")]
        public async Task<ActionResult<ExamResponse>> Update(
            int subjectId,
            int termId,
            int studentId,
            [FromBody, CustomizeValidator(RuleSet = "Update")] UpdateExamRequest req,
            CancellationToken ct)
        {
            if (!User.TryGetPid(out var teacherId))
                return Unauthorized();

            var res = await _svc.UpdateAsync(subjectId, termId, studentId, req, teacherId, ct);
            return Ok(res);
        }
    }
}
