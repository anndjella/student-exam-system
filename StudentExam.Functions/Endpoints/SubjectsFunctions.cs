using Application.DTO.Common;
using Application.DTO.Students;
using Application.DTO.Subjects;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using StudentExam.Functions.Common;

namespace StudentExam.Functions.Endpoints;

public sealed class SubjectsFunctions(ISubjectService _svc, FunctionEndpoint endpoint)
{
    [Function("Subjects_Create")]
    public Task<IActionResult> Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/subjects")]
        HttpRequest request)
        => endpoint.RunAsync<CreateSubjectRequest>(request, EndpointAccess.PasswordChanged("StudentService"), async (req, user) =>
        {
            var resp = await _svc.CreateAsync(req, request.HttpContext.RequestAborted);
            return new CreatedResult($"{request.Scheme}://{request.Host}{request.PathBase}/api/subjects/{resp.ID}", resp);
        }, ruleSet: "Create");

    [Function("Subjects_GetOneById")]
    public Task<IActionResult> GetOneById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/subjects/{id:int}")]
        HttpRequest request, int id)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            var resp = await _svc.GetByIdAsync(id, request.HttpContext.RequestAborted);
            return new OkObjectResult(resp);
        });

    [Function("Subjects_SearchByCode")]
    public Task<IActionResult> SearchByCode(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/subjects/{code}")]
        HttpRequest request, string code)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            var resp = await _svc.GetByCodeAsync(code, request.HttpContext.RequestAborted);
            return new OkObjectResult(resp);
        });

    [Function("Subjects_List")]
    public Task<IActionResult> List(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/subjects")]
        HttpRequest request)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            var active = request.QueryValue<bool>("active", true);
            var skip = request.QueryValue<int>("skip", 0);
            var take = request.QueryValue<int>("take", 20);
            var query = request.QueryValue<string?>("query", null);
            var resp = await _svc.ListPagedAsync(active, skip, take, query, request.HttpContext.RequestAborted);
            return new OkObjectResult(resp);
        });

    [Function("Subjects_ListAllIncludingInactive")]
    public Task<IActionResult> ListAllIncludingInactive(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/subjects/all")]
        HttpRequest request)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            var resp = await _svc.ListAllIncludingInactiveAsync(request.HttpContext.RequestAborted);
            return new OkObjectResult(resp);
        });

    [Function("Subjects_Delete")]
    public Task<IActionResult> Delete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "api/subjects/{id:int}")]
        HttpRequest request, int id)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            await _svc.DeleteAsync(id, request.HttpContext.RequestAborted);
            return new NoContentResult();
        });

    [Function("Subjects_Deactivate")]
    public Task<IActionResult> Deactivate(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "api/subjects/deactivate/{id:int}")]
        HttpRequest request, int id)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            await _svc.DeactivateAsync(id, request.HttpContext.RequestAborted);
            return new NoContentResult();
        });

}
