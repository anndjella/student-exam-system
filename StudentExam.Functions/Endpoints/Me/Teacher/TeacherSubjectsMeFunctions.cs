using Application.Auth;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using StudentExam.Functions.Common;

namespace StudentExam.Functions.Endpoints.Me.Teacher;

public sealed class TeacherSubjectsMeFunctions(ITeachingAssignmentService _svc, FunctionEndpoint endpoint)
{
    [Function("TeacherSubjectsMe_ListMySubjects")]
    public Task<IActionResult> ListMySubjects(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/me/teacher/subjects")]
        HttpRequest request)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("Teacher"), async user =>
        {
            if (!user.TryGetPid(out var personId)) return new UnauthorizedResult();
            
            var res = await _svc.ListTeacherSubjectsDividedAsync(personId, request.HttpContext.RequestAborted);
            return new OkObjectResult(res);
        });

}
