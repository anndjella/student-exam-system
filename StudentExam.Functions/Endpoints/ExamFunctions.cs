using Application.DTO.Exams;
using Application.Services.Interfaces;
using Domain.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using StudentExam.Functions.Common;

namespace StudentExam.Functions.Endpoints;

public sealed class ExamFunctions(IExamService _svc, FunctionEndpoint endpoint)
{
    [Function("Exam_ListPaged")]
    public Task<IActionResult> ListPaged(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/exams/term/{termId:int}/subject/{subjectId:int}")]
        HttpRequest request, int termId, int subjectId)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            var skip = request.QueryValue<int>("skip", 0);
            var take = request.QueryValue<int>("take", 20);
            var query = request.QueryValue<string?>("query", null);
            var res = await _svc.ListPagedAsync(subjectId, termId, skip, take, query, request.HttpContext.RequestAborted);
            return new OkObjectResult(res);
        });

}
