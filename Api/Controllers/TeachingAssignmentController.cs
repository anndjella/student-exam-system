using Application.DTO.TeachingAssignment;
using Application.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Me
{
    [ApiController]
    [Route("api/teaching-assignments")]
    [Authorize(Roles ="StudentService", Policy = "PasswordChanged")]
    public class TeachingAssignmentController : ControllerBase
    {
        private readonly ITeachingAssignmentService _svc;
        public TeachingAssignmentController(ITeachingAssignmentService svc) { _svc = svc; }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody, CustomizeValidator(RuleSet = "Create")] CreateTeachingAssignmentRequest req, CancellationToken ct)
        {
            var resp = await _svc.CreateAsync(req, ct);
            return CreatedAtAction(nameof(GetOneById), new { teacherId = resp.TeacherID, subjectId = resp.SubjectID }, resp);
        }
        [HttpGet("{teacherId:int}/{subjectId:int}")]
        public async Task<ActionResult<TeachingAssignmentResponse>> GetOneById(int teacherId,int subjectId, CancellationToken ct)
        {
            var resp = await _svc.GetAsync(teacherId,subjectId, ct);
            return resp is null ? NotFound() : Ok(resp);
        }
        [HttpPut("{teacherId:int}/{subjectId:int}/can-grade")]
        public async Task<IActionResult> Update(CreateTeachingAssignmentRequest req, CancellationToken ct)
        {
            await _svc.UpdateCanGradeAsync(req, ct);
            return NoContent();
        }

        [HttpDelete("{teacherId:int}/{subjectId:int}")]
        [Authorize(Roles = "StudentService")]
        public async Task<IActionResult> Delete(int teacherId, int subjectId, CancellationToken ct)
        {
            await _svc.DeleteAsync(teacherId, subjectId, ct);
            return NoContent();
        }
    }
}
