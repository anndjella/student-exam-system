using Application.Services.Interfaces;
using Application.Auth;
using Microsoft.AspNetCore.Mvc;
using Application.DTO.Subjects;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using StudentExam.Functions.Common;

namespace StudentExam.Functions.Endpoints.Me.Student;

public sealed class StudentSubjectsMeFunctions(IEnrollmentService _svc, FunctionEndpoint endpoint)
{
    [Function("StudentSubjectsMe_GetMySubjects")]
    public Task<IActionResult> GetMySubjects(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/me/student/subjects/my-enrolled-subjects")]
        HttpRequest request)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("Student"), async user =>
        {
            if (!user.TryGetPid(out var studentId))
                return new UnauthorizedResult();
            
            return new OkObjectResult(await _svc.ListStudentSubjectsAsync(studentId, request.HttpContext.RequestAborted));
        });

    [Function("StudentSubjectsMe_GetMyNotPassed")]
    public Task<IActionResult> GetMyNotPassed(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/me/student/subjects/not-passed-subjects")]
        HttpRequest request)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("Student"), async user =>
        {
            if (!user.TryGetPid(out var studentId))
                return new UnauthorizedResult();
            
            return new OkObjectResult(await _svc.ListNotPassedAsync(studentId, request.HttpContext.RequestAborted));
        });

}
