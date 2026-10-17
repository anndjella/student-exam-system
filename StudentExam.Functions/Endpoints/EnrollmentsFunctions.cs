using Application.DTO.Common;
using Application.DTO.Enrollments;
using Application.DTO.Subjects;
using Application.Services.Interfaces;
using Application.Services.Implementations;
using Domain.Entity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using StudentExam.Functions.Common;

namespace StudentExam.Functions.Endpoints;

public sealed class EnrollmentsFunctions(IEnrollmentService _svc, FunctionEndpoint endpoint)
{
    [Function("Enrollments_BulkEnrollByIndexYear")]
    public Task<IActionResult> BulkEnrollByIndexYear(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/enrollments/bulk-by-index-year")]
        HttpRequest request)
        => endpoint.RunAsync<BulkEnrollByIndexYearRequest>(request, EndpointAccess.PasswordChanged("StudentService"), async (req, user) =>
        {
            var result = await _svc.BulkEnrollByIndexYearAsync(req, request.HttpContext.RequestAborted);
            return new OkObjectResult(result);
        }, ruleSet: "Create");

    [Function("Enrollments_List")]
    public Task<IActionResult> List(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/enrollments/student/index/{index}")]
        HttpRequest request, string index)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            var skip = request.QueryValue<int>("skip", 0);
            var take = request.QueryValue<int>("take", 20);
            var query = request.QueryValue<string?>("query", null);
            var res = await _svc.ListByStudentAsync(index,skip, take, query, request.HttpContext.RequestAborted);
            return new OkObjectResult(res);
        });

    [Function("Enrollments_ListBySubject")]
    public Task<IActionResult> ListBySubject(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/enrollments/subject/code/{code}")]
        HttpRequest request, string code)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            var skip = request.QueryValue<int>("skip", 0);
            var take = request.QueryValue<int>("take", 20);
            var query = request.QueryValue<string?>("query", null);
            var res = await _svc.ListBySubjectAsync(code, skip, take, query, request.HttpContext.RequestAborted);
            return new OkObjectResult(res);
        });

    [Function("Enrollments_Delete")]
    public Task<IActionResult> Delete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "api/enrollments/subject/{subjectId:int}/student/{studentId:int}")]
        HttpRequest request, int subjectId, int studentId)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            await _svc.DeleteAsync(studentId,subjectId,request.HttpContext.RequestAborted);
            return new NoContentResult();
        });

    [Function("Enrollments_Create")]
    public Task<IActionResult> Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/enrollments")]
        HttpRequest request)
        => endpoint.RunAsync<CreateEnrollmentRequest>(request, EndpointAccess.PasswordChanged("StudentService"), async (req, user) =>
        {
            var result = await _svc.CreateAsync(req, request.HttpContext.RequestAborted);
            return new OkObjectResult(result);
        }, ruleSet: "Create");

}
