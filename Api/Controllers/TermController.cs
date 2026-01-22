using Application.DTO.Term;
using Application.Services;
using Domain.Enums;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/terms")]
    [Authorize(Policy = "PasswordChanged")]
    public class TermController : ControllerBase
    {
        private readonly ITermService _svc;
        public TermController(ITermService svc) { _svc = svc; }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody, CustomizeValidator(RuleSet = "Create")] CreateTermRequest req, CancellationToken ct)
        {
            var resp = await _svc.CreateAsync(req, ct);
            return CreatedAtAction(nameof(GetOne), new { id = resp.TermID }, resp);
        }

        [HttpGet("{id:int}")]
        //[Authorize(Roles = "StudentService,Teacher,Student")]
        [Authorize(Roles = "StudentService")]

        public async Task<ActionResult<TermResponse>> GetOne(int id, CancellationToken ct)
        {
            var resp = await _svc.GetByIdAsync(id, ct);
            return resp is null ? NotFound() : Ok(resp);
        }

        [HttpGet]
        [Authorize(Roles = "StudentService,Teacher,Student")]
        public async Task<ActionResult<List<TermResponse>>> List(CancellationToken ct)
        {
            if (User.IsInRole("StudentService"))
                return Ok(await _svc.ListAsync(UserRole.StudentService, ct));

            if (User.IsInRole("Teacher"))
                return Ok(await _svc.ListAsync(UserRole.Teacher, ct));

            if (User.IsInRole("Student"))
                return Ok(await _svc.ListAsync(UserRole.Student, ct));

            return Forbid();
        }
        [HttpGet("for-grading")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<List<TermResponse>>> ListForGrading(CancellationToken ct)
        {
           var resp=await _svc.ListForGradingAsync(ct);
           return resp is null ? NotFound() : Ok(resp);
        }
        [HttpGet("open-for-registration")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<List<TermResponse>>> ListOpenForRegistration(CancellationToken ct)
        {
            var resp = await _svc.ListOpenForRegistrationAsync(ct);
            return resp is null ? NotFound() : Ok(resp);
        }
        [HttpDelete("{id:int}")]
        [Authorize(Roles ="StudentService")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _svc.DeleteAsync(id, ct);
            return NoContent();
        }

    }
}
