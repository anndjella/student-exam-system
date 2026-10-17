using Application.Auth;
using Application.DTO.Exams;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using StudentExam.Functions.Common;

namespace StudentExam.Functions.Endpoints.Me.Student;

public sealed class StudentExamsMeFunctions(IExamService _svc, FunctionEndpoint endpoint)
{
    [Function("StudentExamsMe_GetSigned")]
    public Task<IActionResult> GetSigned(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/me/student/exams/signed")]
        HttpRequest request)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("Student"), async user =>
        {
            if (!user.TryGetPid(out var personId))
                return new UnauthorizedResult();
            
            var res = await _svc.ListMySignedAsync(personId, request.HttpContext.RequestAborted);
            return new OkObjectResult(res);
        });

}
