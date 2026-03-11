using Application.DTO.Exams;
using Application.DTO.Me.StudService;
using Application.Services;
using Domain.Entity;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/exams")]
    [Authorize(Roles ="StudentService",Policy ="PasswordChanged")]
    public sealed class ExamController : ControllerBase
    {
        private readonly IExamService _svc;
        public ExamController(IExamService svc) => _svc = svc;
        [HttpGet("term/{termId:int}/subject/{subjectId:int}")]
        public async Task<ActionResult<StudentServiceExamsResponse>> ListPaged(
          int subjectId,
          int termId,
          [FromQuery] int skip = 0,
          [FromQuery] int take = 20,
          [FromQuery] string? query = null,
          CancellationToken ct = default)
        {
            var res = await _svc.ListPagedAsync(subjectId, termId, skip, take, query, ct);
            return Ok(res);
        }
    }
}
