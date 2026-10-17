using Application.DTO.TeachingAssignment;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using StudentExam.Functions.Common;

namespace StudentExam.Functions.Endpoints;

public sealed class TeachingAssignmentFunctions(ITeachingAssignmentService _svc, FunctionEndpoint endpoint)
{
    [Function("TeachingAssignment_Create")]
    public Task<IActionResult> Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/teaching-assignments")]
        HttpRequest request)
        => endpoint.RunAsync<CreateTeachingAssignmentRequest>(request, EndpointAccess.PasswordChanged("StudentService"), async (req, user) =>
        {
            var resp = await _svc.CreateAsync(req, request.HttpContext.RequestAborted);
            return new CreatedResult($"{request.Scheme}://{request.Host}{request.PathBase}/api/teaching-assignments/teacher/{resp.TeacherID}/subject/{resp.SubjectID}", resp);
        }, ruleSet: "Create");

    [Function("TeachingAssignment_GetOneById")]
    public Task<IActionResult> GetOneById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/teaching-assignments/teacher/{teacherId:int}/subject/{subjectId:int}")]
        HttpRequest request, int teacherId, int subjectId)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            var resp = await _svc.GetAsync(teacherId,subjectId, request.HttpContext.RequestAborted);
            return new OkObjectResult(resp);
        });

    [Function("TeachingAssignment_ListByTeacherId")]
    public Task<IActionResult> ListByTeacherId(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/teaching-assignments/by-teacher/{teacherId:int}")]
        HttpRequest request, int teacherId)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            var resp = await _svc.ListByTeacherAsync(teacherId, request.HttpContext.RequestAborted);
            return new OkObjectResult(resp);
        });

    [Function("TeachingAssignment_ListBySubjectId")]
    public Task<IActionResult> ListBySubjectId(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/teaching-assignments/by-subject/{subjectId:int}")]
        HttpRequest request, int subjectId)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            var resp = await _svc.ListBySubjectAsync(subjectId, request.HttpContext.RequestAborted);
            return new OkObjectResult(resp);
        });

    [Function("TeachingAssignment_Update")]
    public Task<IActionResult> Update(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "api/teaching-assignments/teacher/{teacherId:int}/subject/{subjectId:int}/can-grade")]
        HttpRequest request)
        => endpoint.RunAsync<CreateTeachingAssignmentRequest>(request, EndpointAccess.PasswordChanged("StudentService"), async (req, user) =>
        {
            await _svc.UpdateCanGradeAsync(req, request.HttpContext.RequestAborted);
            return new NoContentResult();
        });

    [Function("TeachingAssignment_Delete")]
    public Task<IActionResult> Delete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "api/teaching-assignments/teacher/{teacherId:int}/subject/{subjectId:int}")]
        HttpRequest request, int teacherId, int subjectId)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            await _svc.DeleteAsync(teacherId, subjectId, request.HttpContext.RequestAborted);
            return new NoContentResult();
        });

}
