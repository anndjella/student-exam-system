using Application.Auth;
using Application.DTO.Exams;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using StudentExam.Functions.Common;

namespace StudentExam.Functions.Endpoints.Me.Teacher;

public sealed class TeacherExamsMeFunctions(IExamService _svc, FunctionEndpoint endpoint)
{
    [Function("TeacherExamsMe_Create")]
    public Task<IActionResult> Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/me/teacher/exams/subject/{subjectId:int}/term/{termId:int}/student/{studentId:int}")]
        HttpRequest request, int subjectId, int termId, int studentId)
        => endpoint.RunAsync<CreateExamRequest>(request, EndpointAccess.PasswordChanged("Teacher"), async (req, user) =>
        {
            if (!user.TryGetPid(out var teacherId))
                return new UnauthorizedResult();
            
            var res = await _svc.CreateAsync(subjectId, termId, studentId, req, teacherId, request.HttpContext.RequestAborted);
            return new OkObjectResult(res);
        }, ruleSet: "Create");

    [Function("TeacherExamsMe_Lock")]
    public Task<IActionResult> Lock(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/me/teacher/exams/lock")]
        HttpRequest request)
        => endpoint.RunAsync<LockExamsRequest>(request, EndpointAccess.PasswordChanged("Teacher"), async (req, user) =>
        {
            var teacherId = int.Parse(user.FindFirst("pid")!.Value);
            var lockedCount = await _svc.LockAsync(req, teacherId, request.HttpContext.RequestAborted);
            return new OkObjectResult(lockedCount);
        });

    [Function("TeacherExamsMe_Update")]
    public Task<IActionResult> Update(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "api/me/teacher/exams/{examId:int}")]
        HttpRequest request, int examId)
        => endpoint.RunAsync<UpdateExamRequest>(request, EndpointAccess.PasswordChanged("Teacher"), async (req, user) =>
        {
            if (!user.TryGetPid(out var teacherId))
                return new UnauthorizedResult();
            
            var res = await _svc.UpdateAsync(examId, req, teacherId, request.HttpContext.RequestAborted);
            return new OkObjectResult(res);
        }, ruleSet: "Update");

    [Function("TeacherExamsMe_ListBySubjectTerm")]
    public Task<IActionResult> ListBySubjectTerm(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/me/teacher/exams/term/{termId:int}/subject/{subjectId:int}")]
        HttpRequest request, int termId, int subjectId)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("Teacher"), async user =>
        {
            if (!user.TryGetPid(out var teacherId))
                return new UnauthorizedResult();
            var res = await _svc.ListBySubjectTermAsync(subjectId,termId,teacherId, request.HttpContext.RequestAborted);
            return new OkObjectResult(res);
        });

}
