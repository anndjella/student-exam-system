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
        public async Task<ActionResult<StudentServiceExamsResponse>> ListAll(int subjectId,int termId,
           CancellationToken ct)
        {
            var res = await _svc.ListAllBySubjectTermAsync(subjectId, termId, ct);
            return Ok(res);
        }
    }
}
