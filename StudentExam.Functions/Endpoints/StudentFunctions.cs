using Application.DTO.Common;
using Application.DTO.Exams;
using Application.DTO.Students;
using Application.Services.Interfaces;
using Domain.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using StudentExam.Functions.Common;

namespace StudentExam.Functions.Endpoints;

public sealed class StudentFunctions(IStudentService _svc, FunctionEndpoint endpoint)
{
    [Function("Student_Create")]
    public Task<IActionResult> Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/students")]
        HttpRequest request)
        => endpoint.RunAsync<CreateStudentRequest>(request, EndpointAccess.PasswordChanged("StudentService"), async (req, user) =>
        {
            var resp = await _svc.CreateAsync(req, request.HttpContext.RequestAborted);
            return new CreatedResult($"{request.Scheme}://{request.Host}{request.PathBase}/api/students/{resp.ID}", resp);
        }, ruleSet: "Create");

    [Function("Student_GetOneById")]
    public Task<IActionResult> GetOneById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/students/{id:int}")]
        HttpRequest request, int id)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService", "Teacher"), async user =>
        {
            var resp = await _svc.GetByIdAsync(id, request.HttpContext.RequestAborted);
            return new OkObjectResult(resp);
        });

    [Function("Student_GetOneByIndex")]
    public Task<IActionResult> GetOneByIndex(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/students/{year:int}/{number:int}")]
        HttpRequest request, int year, int number)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService", "Teacher"), async user =>
        {
            string index = $"{year}/{number:D4}";
            var resp= await _svc.GetByIndexAsync(index,request.HttpContext.RequestAborted);
            return new OkObjectResult(resp);
        });

    [Function("Student_Update")]
    public Task<IActionResult> Update(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "api/students/{id:int}")]
        HttpRequest request, int id)
        => endpoint.RunAsync<UpdateStudentRequest>(request, EndpointAccess.PasswordChanged("StudentService"), async (req, user) =>
        {
            await _svc.UpdateAsync(id, req, request.HttpContext.RequestAborted);
            return new NoContentResult();
        }, ruleSet: "Update");

    [Function("Student_SoftDelete")]
    public Task<IActionResult> SoftDelete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "api/students/{id:int}")]
        HttpRequest request, int id)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            await _svc.SoftDeleteAsync(id, request.HttpContext.RequestAborted);
            return new NoContentResult();
        });

    [Function("Student_List")]
    public Task<IActionResult> List(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/students")]
        HttpRequest request)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService", "Teacher"), async user =>
        {
            var skip = request.QueryValue<int>("skip", 0);
            var take = request.QueryValue<int>("take", 20);
            var query = request.QueryValue<string?>("query", null);
            var onlyDeleted = request.QueryValue<bool>("onlyDeleted", false);
            var res = await _svc.ListAsync(skip, take, query,onlyDeleted, request.HttpContext.RequestAborted);
            return new OkObjectResult(res);
        });

}
