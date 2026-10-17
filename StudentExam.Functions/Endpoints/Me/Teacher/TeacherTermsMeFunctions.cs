using Application.Auth;
using Application.DTO.Term;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.Pkcs;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using StudentExam.Functions.Common;

namespace StudentExam.Functions.Endpoints.Me.Teacher;

public sealed class TeacherTermsMeFunctions(ITermService _svc, FunctionEndpoint endpoint)
{
    [Function("TeacherTermsMe_ListForGrading")]
    public Task<IActionResult> ListForGrading(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/me/teacher/terms/for-grading/subject/{subjectId:int}")]
        HttpRequest request, int subjectId)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("Teacher"), async user =>
        {
            if (!user.TryGetPid(out var personId)) return new UnauthorizedResult();
            var resp = await _svc.ListForGradingAsync(personId, subjectId, request.HttpContext.RequestAborted);
            return new OkObjectResult(resp);
        });

}
