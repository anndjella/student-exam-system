using Application.DTO.Exams;
using Application.DTO.Students;
using Application.DTO.Teachers;
using Application.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/teachers")]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _svc;
        public TeacherController(ITeacherService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody, CustomizeValidator(RuleSet = "Create")] CreateTeacherRequest req, CancellationToken ct)
        {

            var resp = await _svc.CreateAsync(req, ct);
            return CreatedAtAction(nameof(GetOne), new { id = resp.Id }, resp);

        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id, CancellationToken ct)
        {
            var resp = await _svc.GetAsync(id, ct);
            return resp is null ? NotFound() : Ok(resp);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
            => Ok(await _svc.ListAsync(ct));

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody, CustomizeValidator(RuleSet = "Update")] UpdateTeacherRequest req, CancellationToken ct)
        {
            await _svc.UpdateAsync(id, req, ct); return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _svc.DeleteAsync(id, ct);
            return NoContent();
        }
        [HttpGet("{id:int}/examsAsExaminer")]
        public async Task<ActionResult<IReadOnlyList<ExamResponse>>> GetExamsAsExaminer(
        int id,
        CancellationToken ct)
        {
            var exams = await _svc.ListExamsAsExaminerAsync(id, ct);
            return Ok(exams);
        }
        [HttpGet("{id:int}/examsAsSupervisor")]
        public async Task<ActionResult<IReadOnlyList<ExamResponse>>> GetExamsAsSupervisor(
        int id,
        CancellationToken ct)
        {
            var exams = await _svc.ListExamsAsSupervisorAsync(id, ct);
            return Ok(exams);
        }
    }
}
