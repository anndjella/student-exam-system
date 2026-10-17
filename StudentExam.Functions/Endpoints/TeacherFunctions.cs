using Application.DTO.Common;
using Application.DTO.Exams;
using Application.DTO.Students;
using Application.DTO.Teachers;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using StudentExam.Functions.Common;

namespace StudentExam.Functions.Endpoints;

public sealed class TeacherFunctions(ITeacherService _svc, FunctionEndpoint endpoint)
{
    [Function("Teacher_Create")]
    public Task<IActionResult> Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/teachers")]
        HttpRequest request)
        => endpoint.RunAsync<CreateTeacherRequest>(request, EndpointAccess.PasswordChanged("StudentService"), async (req, user) =>
        {
            var resp = await _svc.CreateAsync(req, request.HttpContext.RequestAborted);
            return new CreatedResult($"{request.Scheme}://{request.Host}{request.PathBase}/api/teachers/{resp.ID}", resp);
        }, ruleSet: "Create");

    [Function("Teacher_GetOneById")]
    public Task<IActionResult> GetOneById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/teachers/{id:int}")]
        HttpRequest request, int id)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            var resp = await _svc.GetByIdAsync(id, request.HttpContext.RequestAborted);
            return new OkObjectResult(resp);
        });

    [Function("Teacher_GetOneByNum")]
    public Task<IActionResult> GetOneByNum(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/teachers/year/{year:int}/number/{number:int}")]
        HttpRequest request, int year, int number)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            string employeeNum=$"{year}/{number:D4}";
            var resp = await _svc.GetByNumAsync(employeeNum, request.HttpContext.RequestAborted);
            return new OkObjectResult(resp);
        });

    [Function("Teacher_Update")]
    public Task<IActionResult> Update(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "api/teachers/{id:int}")]
        HttpRequest request, int id)
        => endpoint.RunAsync<UpdateTeacherRequest>(request, EndpointAccess.PasswordChanged("StudentService"), async (req, user) =>
        {
            await _svc.UpdateAsync(id, req, request.HttpContext.RequestAborted); 
            return new NoContentResult();
        }, ruleSet: "Update");

    [Function("Teacher_SoftDelete")]
    public Task<IActionResult> SoftDelete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "api/teachers/{id:int}")]
        HttpRequest request, int id)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            await _svc.SoftDeleteAsync(id, request.HttpContext.RequestAborted);
            return new NoContentResult();
        });

    [Function("Teacher_List")]
    public Task<IActionResult> List(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/teachers")]
        HttpRequest request)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            var skip = request.QueryValue<int>("skip", 0);
            var take = request.QueryValue<int>("take", 20);
            var query = request.QueryValue<string?>("query", null);
            var onlyDeleted = request.QueryValue<bool>("onlyDeleted", false);
            var res = await _svc.ListAsync(skip, take, query, onlyDeleted, request.HttpContext.RequestAborted);
            return new OkObjectResult(res);
        });

}
