using Application.DTO.Students;
using Application.DTO.Teachers;
using Application.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/teachers")]
    public class TeacherController : Controller
    {
        private readonly ITeacherService _svc;
        public TeacherController(ITeacherService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTeacherRequest req, CancellationToken ct)
        {
            try
            {
                var resp = await _svc.CreateAsync(req, ct);
                return CreatedAtAction(nameof(GetOne), new { id = resp.Id }, resp);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTeacherRequest req, CancellationToken ct)
        {
            try { await _svc.UpdateAsync(id, req, ct); return NoContent(); }
            catch (KeyNotFoundException)
            {
                return NotFound($"Person with id {id} does not exist.");
            }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _svc.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
