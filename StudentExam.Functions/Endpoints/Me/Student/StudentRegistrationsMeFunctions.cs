using Application.Auth;
using Application.DTO.Registrations;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using StudentExam.Functions.Common;

namespace StudentExam.Functions.Endpoints.Me.Student;

public sealed class StudentRegistrationsMeFunctions(IRegistrationService _svc, FunctionEndpoint endpoint)
{
    [Function("StudentRegistrationsMe_Create")]
    public Task<IActionResult> Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/me/student/registrations/registrations")]
        HttpRequest request)
        => endpoint.RunAsync<CreateRegistrationRequest>(request, EndpointAccess.PasswordChanged("Student"), async (req, user) =>
        {
            if (!user.TryGetPid(out var studentId))
                return new UnauthorizedResult();
            
            var resp = await _svc.CreateAsync(studentId, req, request.HttpContext.RequestAborted);
            return new OkObjectResult(resp);
        }, ruleSet: "Create");

    [Function("StudentRegistrationsMe_Cancel")]
    public Task<IActionResult> Cancel(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "api/me/student/registrations/cancel/subject/{subjectId:int}/term/{termId:int}")]
        HttpRequest request, int subjectId, int termId)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("Student"), async user =>
        {
            if (!user.TryGetPid(out var studentId))
                return new UnauthorizedResult();
            
            await _svc.CancelAsync(studentId, subjectId, termId, request.HttpContext.RequestAborted);
            return new NoContentResult();
        });

    [Function("StudentRegistrationsMe_List")]
    public Task<IActionResult> List(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/me/student/registrations/my-active-registrations")]
        HttpRequest request)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("Student"), async user =>
        {
            if (!user.TryGetPid(out var studentId))
                return new UnauthorizedResult();
            
            var list = await _svc.ListMyActiveAsync(studentId, request.HttpContext.RequestAborted);
            return new OkObjectResult(list);
        });

}
