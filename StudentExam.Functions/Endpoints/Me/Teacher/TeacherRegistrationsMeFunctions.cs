using Application.Auth;
using Application.DTO.Registrations;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using StudentExam.Functions.Common;

namespace StudentExam.Functions.Endpoints.Me.Teacher;

public sealed class TeacherRegistrationsMeFunctions(IRegistrationService _svc, FunctionEndpoint endpoint)
{
    [Function("TeacherRegistrationsMe_ListForSubjectAndTerm")]
    public Task<IActionResult> ListForSubjectAndTerm(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/me/teacher/registrations/subject/{subjectId:int}/term/{termId:int}")]
        HttpRequest request, int subjectId, int termId)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("Teacher"), async user =>
        {
            if (!user.TryGetPid(out var personId)) return new UnauthorizedResult();
            
            var res = await _svc.ListMyActiveBySubjectAndTermAsync(personId, subjectId, termId, request.HttpContext.RequestAborted);
            return new OkObjectResult(res);
        });

}
