using Application.DTO.Exams;
using Application.Services;
using Domain.Entity;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/exams")]
    public sealed class ExamController : ControllerBase
    {
        private readonly IExamService _svc;
        public ExamController(IExamService svc) => _svc = svc;

        //[HttpPost]
        //public async Task<IActionResult> Create(
        //    [FromBody, CustomizeValidator(RuleSet = "Create")] CreateExamRequest req,
        //    CancellationToken ct)
        //{
        //    await _svc.CreateAsync(req, ct);
        //    return NoContent();
        //}

        ////[HttpGet("{studentId:int}/{subjectId:int}/{date}")]
        ////public async Task<IActionResult> GetOne(int studentId, int subjectId, DateOnly date, CancellationToken ct)
        ////{
        ////    var resp = await _svc.GetAsync(studentId, subjectId, date, ct);
        ////    return resp is null ? NotFound() : Ok(resp);
        ////}
        ////[HttpGet]
        ////public async Task<IActionResult> GetAll(CancellationToken ct)
        ////=> Ok(await _svc.ListAsync(ct));

        //[HttpPut("{studentId:int}/{subjectId:int}/{date}")]
        //public async Task<IActionResult> Update(
        //    int studentId, int subjectId, DateOnly date,
        //    [FromBody, CustomizeValidator(RuleSet = "Update")] UpdateExamRequest req, CancellationToken ct)
        //{ await _svc.UpdateAsync(studentId, subjectId, date, req, ct); return NoContent(); }


        //[HttpDelete("{studentId:int}/{subjectId:int}/{date}")]
        //public async Task<IActionResult> Delete(int studentId, int subjectId, DateOnly date, CancellationToken ct)
        //{
        //    await _svc.DeleteAsync(studentId, subjectId, date, ct);
        //    return NoContent();
        //}
    }
}
