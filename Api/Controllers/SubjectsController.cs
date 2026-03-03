using Application.DTO.Common;
using Application.DTO.Students;
using Application.DTO.Subjects;
using Application.Services;
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
        public async Task<ActionResult<AdminSubjectResponse>> GetOneById(int id, CancellationToken ct)
        {
            var resp = await _svc.GetByIdAsync(id, ct);
            return resp is null ? NotFound() : Ok(resp);
        }
        [HttpGet("{code}")]
        public async Task<ActionResult<AdminSubjectResponse>> SearchByCode(string code, CancellationToken ct)
        {
            var resp = await _svc.GetByCodeAsync(code, ct);
            return resp is null ? NotFound() : Ok(resp);
        }
        //[HttpGet("all")]
        //public async Task<ActionResult<AdminSubjectsResponse>> ListAll(CancellationToken ct)
        //{
        //    var resp = await _svc.ListAllWithTeachersAsync(ct);
        //    return resp is null ? NotFound() : Ok(resp);
        //}
        [HttpGet]
        public async Task<ActionResult<PagedResponse<AdminSubjectResponse>>> List(
            [FromQuery] bool active = true,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 20,
            [FromQuery] string? query = null,
            CancellationToken ct = default)
        {
            if (skip < 0) skip = 0;
            if (take <= 0) take = 20;
            if (take > 100) take = 100;

            var resp = await _svc.ListPagedAsync(active, skip, take, query, ct);
            return Ok(resp);
        }

        [HttpGet("active")]
        public async Task<ActionResult<List<SubjectResponse>>> ListActive(CancellationToken ct)
        {
            var resp = await _svc.ListActiveAsync(ct);
            return resp is null ? NotFound() : Ok(resp);
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
