using Application.DTO.Common;
using Application.DTO.Enrollments;
using Application.DTO.Subjects;
using Application.Services;
using Application.ServicesImplementation;
using Domain.Entity;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    [Authorize(Roles = "StudentService", Policy = "PasswordChanged")]
    public sealed class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentService _svc;

        public EnrollmentsController(IEnrollmentService svc) => _svc = svc;

        [HttpPost("bulk-by-index-year")]
        public async Task<ActionResult<BulkEnrollResult>> BulkEnrollByIndexYear(
             [FromBody, CustomizeValidator(RuleSet = "Create")] BulkEnrollByIndexYearRequest req,
            CancellationToken ct)
        {
            var result = await _svc.BulkEnrollByIndexYearAsync(req, ct);
            return Ok(result);
        }
        //[HttpGet]
        //public async Task<ActionResult<PagedResponse<EnrollmentResponse>>> List(
        //        [FromQuery] int skip = 0,
        //        [FromQuery] int take = 20,
        //        [FromQuery] string? query = null,
        //        CancellationToken ct = default)
        //{
        //    if (skip < 0) skip = 0;
        //    if (take <= 0) take = 20;
        //    if (take > 100) take = 100;

        //    var res = await _svc.ListAsync(skip, take, query, ct);
        //    return Ok(res);
        //}
        [HttpGet("student/index/{index}")]
        public async Task<ActionResult<PagedResponse<EnrollmentResponse>>> List(
               [FromRoute] string index,
               [FromQuery] int skip = 0,
               [FromQuery] int take = 20,
               [FromQuery] string? query = null,
               CancellationToken ct = default)
        {
            if (skip < 0) skip = 0;
            if (take <= 0) take = 20;
            if (take > 100) take = 100;

            var res = await _svc.ListByStudentAsync(index,skip, take, query, ct);
            return Ok(res);
        }
        [HttpGet("subject/code/{code}")]
        public async Task<ActionResult<PagedResponse<EnrollmentResponse>>> ListBySubject(
               [FromRoute] string code,
               [FromQuery] int skip = 0,
               [FromQuery] int take = 20,
               [FromQuery] string? query = null,
               CancellationToken ct = default)
        {
            if (skip < 0) skip = 0;
            if (take <= 0) take = 20;
            if (take > 100) take = 100;

            var res = await _svc.ListBySubjectAsync(code, skip, take, query, ct);
            return Ok(res);
        }
        [HttpDelete("subject/{subjectId:int}/student/{studentId:int}")]
        public async Task<IActionResult> Delete(int subjectId,int studentId, CancellationToken ct=default)
        {
            await _svc.DeleteAsync(studentId,subjectId,ct);
            return NoContent();
        }
        [HttpPost]

        public async Task<ActionResult<EnrollmentResponse>> Create([FromBody, CustomizeValidator(RuleSet = "Create")] CreateEnrollmentRequest req,CancellationToken ct)
        {
            var result = await _svc.CreateAsync(req, ct);
            return Ok(result);
        }

    }
}
