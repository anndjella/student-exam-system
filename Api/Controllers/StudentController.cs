using Application.DTO.Exams;
using Application.DTO.Students;
using Application.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{

    [ApiController]
    [Route("api/students")]
    public sealed class StudentController : ControllerBase
    {
        private readonly IStudentService _svc;
        public StudentController(IStudentService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody, CustomizeValidator(RuleSet = "Create")] CreateStudentRequest req, CancellationToken ct)
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
        public async Task<IActionResult> Update(int id, [FromBody, CustomizeValidator(RuleSet = "Update")] UpdateStudentRequest req, CancellationToken ct)
        {
            await _svc.UpdateAsync(id, req, ct);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _svc.SoftDeleteAsync(id, ct);
            return NoContent();
        }
    }
}
