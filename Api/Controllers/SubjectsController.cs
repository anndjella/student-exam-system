using Application.DTO.Students;
using Application.DTO.Subjects;
using Application.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/subjects")]
    public class SubjectsController : ControllerBase
    {
        private readonly ISubjectService _svc;
        public SubjectsController(ISubjectService svc)
        {
            _svc = svc;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
            => Ok(await _svc.ListAsync(ct));
        [HttpPost]
        public async Task<IActionResult> Create([FromBody, CustomizeValidator(RuleSet = "Create")] CreateSubjectRequest req, CancellationToken ct)
        {
            var resp = await _svc.CreateAsync(req, ct);
            return CreatedAtAction(nameof(GetOne), new { id = resp.ID }, resp);

        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id, CancellationToken ct)
        {
            var resp = await _svc.GetAsync(id, ct);
            return resp is null ? NotFound() : Ok(resp);
        }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody, CustomizeValidator(RuleSet = "Update")] UpdateSubjectRequest req, CancellationToken ct)
        {
            await _svc.UpdateAsync(id, req, ct); return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _svc.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
