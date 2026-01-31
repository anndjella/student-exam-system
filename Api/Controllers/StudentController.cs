using Application.DTO.Common;
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
        [Authorize(Roles = "StudentService")]
        public async Task<IActionResult> Create([FromBody, CustomizeValidator(RuleSet = "Create")] CreateStudentRequest req, CancellationToken ct)
        {
            var resp = await _svc.CreateAsync(req, ct);
            return CreatedAtAction(nameof(GetOneById), new { id = resp.ID }, resp);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "StudentService,Teacher")]
        public async Task<ActionResult<StudentResponse>> GetOneById(int id, CancellationToken ct)
        {
            var resp = await _svc.GetByIdAsync(id, ct);
            return resp is null ? NotFound() : Ok(resp);
        }
        [HttpGet("{year:int}/{number:int}")]
        [Authorize(Roles ="StudentService,Teacher")]
        public async Task<ActionResult<StudentResponse>> GetOneByIndex(int year, int number, CancellationToken ct)
        {
            string index = $"{year}/{number:D4}";
            var resp= await _svc.GetByIndexAsync(index,ct);
            return resp is null ? NotFound() : Ok( resp);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "StudentService")]
        public async Task<IActionResult> Update(int id, [FromBody, CustomizeValidator(RuleSet = "Update")] UpdateStudentRequest req, CancellationToken ct)
        {
            await _svc.UpdateAsync(id, req, ct);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "StudentService")]
        public async Task<IActionResult> SoftDelete(int id, CancellationToken ct)
        {
            await _svc.SoftDeleteAsync(id, ct);
            return NoContent();
        }
        [HttpGet]
        [Authorize(Roles = "StudentService,Teacher")]
        public async Task<ActionResult<PagedResponse<StudentResponse>>> List(
                [FromQuery] int skip = 0,
                [FromQuery] int take = 20,
                [FromQuery] string? query = null,
                CancellationToken ct = default)
        {
            if (skip < 0) skip = 0;
            if (take <= 0) take = 20;
            if (take > 100) take = 100;

            var res = await _svc.ListAsync(skip, take, query, ct);
            return Ok(res);
        }
    }
}
