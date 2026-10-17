using Application.DTO.Term;
using Application.Services.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using StudentExam.Functions.Common;

namespace StudentExam.Functions.Endpoints;

public sealed class TermFunctions(ITermService _svc, FunctionEndpoint endpoint)
{
    [Function("Term_Create")]
    public Task<IActionResult> Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/terms")]
        HttpRequest request)
        => endpoint.RunAsync<CreateTermRequest>(request, EndpointAccess.PasswordChanged(), async (req, user) =>
        {
            var resp = await _svc.CreateAsync(req, request.HttpContext.RequestAborted);
            return new CreatedResult($"{request.Scheme}://{request.Host}{request.PathBase}/api/terms/{resp.TermID}", resp);
        }, ruleSet: "Create");

    [Function("Term_GetOne")]
    public Task<IActionResult> GetOne(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/terms/{id:int}")]
        HttpRequest request, int id)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            var resp = await _svc.GetByIdAsync(id, request.HttpContext.RequestAborted);
            return new OkObjectResult(resp);
        });

    [Function("Term_List")]
    public Task<IActionResult> List(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/terms")]
        HttpRequest request)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService", "Teacher", "Student"), async user =>
        {
            if (user.IsInRole("StudentService"))
                return new OkObjectResult(await _svc.ListAsync(UserRole.StudentService, request.HttpContext.RequestAborted));
            
            if (user.IsInRole("Teacher"))
                return new OkObjectResult(await _svc.ListAsync(UserRole.Teacher, request.HttpContext.RequestAborted));
            
            if (user.IsInRole("Student"))
                return new OkObjectResult(await _svc.ListAsync(UserRole.Student, request.HttpContext.RequestAborted));
            
            return new StatusCodeResult(403);
        });

    [Function("Term_ListForExamsView")]
    public Task<IActionResult> ListForExamsView(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/terms/for-exams-view")]
        HttpRequest request)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("Teacher"), async user =>
        {
            var resp = await _svc.ListForTeacherExamsViewAsync(request.HttpContext.RequestAborted);
            return new OkObjectResult(resp);
        });

    [Function("Term_ListOpenForRegistration")]
    public Task<IActionResult> ListOpenForRegistration(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/terms/open-for-registration")]
        HttpRequest request)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("Student"), async user =>
        {
            var resp = await _svc.ListOpenForRegistrationAsync(request.HttpContext.RequestAborted);
            return new OkObjectResult(resp);
        });

    [Function("Term_Delete")]
    public Task<IActionResult> Delete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "api/terms/{id:int}")]
        HttpRequest request, int id)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            await _svc.DeleteAsync(id, request.HttpContext.RequestAborted);
            return new NoContentResult();
        });

}
