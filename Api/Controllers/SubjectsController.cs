using Application.DTO.Common;
using Application.DTO.Students;
using Application.DTO.Subjects;
using Application.Services.Interfaces;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/subjects")]
    [Authorize(Roles = "StudentService", Policy = "PasswordChanged")]
    public class SubjectsController : ControllerBase
    {
        private readonly ISubjectService _svc;
        public SubjectsController(ISubjectService svc)
        {
            _svc = svc;
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody, CustomizeValidator(RuleSet = "Create")] CreateSubjectRequest req, CancellationToken ct)
        {
            var resp = await _svc.CreateAsync(req, ct);
            return CreatedAtAction(nameof(GetOneById), new { id = resp.ID }, resp);

        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<StudServiceSubjectResponse>> GetOneById(int id, CancellationToken ct)
        {
            var resp = await _svc.GetByIdAsync(id, ct);
            return Ok(resp);
        }
        [HttpGet("{code}")]
        public async Task<ActionResult<StudServiceSubjectResponse>> SearchByCode(string code, CancellationToken ct)
        {
            var resp = await _svc.GetByCodeAsync(code, ct);
            return Ok(resp);
        }
        [HttpGet]
        public async Task<ActionResult<PagedResponse<StudServiceSubjectResponse>>> List(
            [FromQuery] bool active = true,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 20,
            [FromQuery] string? query = null,
            CancellationToken ct = default)
        {
            var resp = await _svc.ListPagedAsync(active, skip, take, query, ct);
            return Ok(resp);
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<SimpleSubjectResponse>>> ListAllIncludingInactive(CancellationToken ct)
        {
            var resp = await _svc.ListAllIncludingInactiveAsync(ct);
            return Ok(resp);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _svc.DeleteAsync(id, ct);
            return NoContent();
        }
        [HttpPatch("deactivate/{id:int}")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
        {
            await _svc.DeactivateAsync(id, ct);
            return NoContent();
        }

    }
}
