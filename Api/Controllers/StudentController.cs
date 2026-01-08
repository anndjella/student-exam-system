using Application.DTO.Exams;
using Application.DTO.Students;
using Application.Services;
using Domain.Entity;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{

    [ApiController]
    [Route("api/students")]
    [Authorize(Policy = "PasswordChanged")]
    public sealed class StudentController : ControllerBase
    {
        private readonly IStudentService _svc;
        public StudentController(IStudentService svc) => _svc = svc;

        [HttpPost]
        [Authorize(Roles = "StudentService", Policy = "PasswordChanged")]
        public async Task<IActionResult> Create(CreateStudentRequest req, CancellationToken ct)
        {
            var resp = await _svc.CreateAsync(req, ct);
            return CreatedAtAction(nameof(GetOne), new { id = resp.ID }, resp);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "StudentService,Teacher", Policy = "PasswordChanged")]
        public async Task<ActionResult<StudentResponse>> GetOne(int id, CancellationToken ct)
        {
            var resp = await _svc.GetByIdAsync(id, ct);
            return resp is null ? NotFound() : Ok(resp);
        }
        [HttpGet("{year:int}/{number:int}")]
        [Authorize(Roles ="StudentService,Teacher", Policy="PasswordChanged")]
        public async Task<ActionResult<StudentResponse>> GetOneByIndex(int year, int number, CancellationToken ct)
        {
            string index = $"{year}/{number:D4}";
            var resp= await _svc.GetByIndexAsync(index,ct);
            return resp is null ? NotFound() : Ok( resp);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "StudentService", Policy = "PasswordChanged")]
        public async Task<IActionResult> Update(int id, UpdateStudentRequest req, CancellationToken ct)
        {
            await _svc.UpdateAsync(id, req, ct);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "StudentService", Policy = "PasswordChanged")]
        public async Task<IActionResult> SoftDelete(int id, CancellationToken ct)
        {
            await _svc.SoftDeleteAsync(id, ct);
            return NoContent();
        }
    }
}
