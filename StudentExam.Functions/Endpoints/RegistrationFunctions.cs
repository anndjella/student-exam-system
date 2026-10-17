using Application.DTO.Common;
using Application.DTO.Enrollments;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using StudentExam.Functions.Common;

namespace StudentExam.Functions.Endpoints;

public sealed class RegistrationFunctions(IRegistrationService _svc, FunctionEndpoint endpoint)
{
    [Function("Registration_List")]
    public Task<IActionResult> List(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/registrations")]
        HttpRequest request)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("StudentService"), async user =>
        {
            var subjectId = request.QueryValue<int>("subjectId", default);
            var termId = request.QueryValue<int>("termId", default);
            var skip = request.QueryValue<int>("skip", 0);
            var take = request.QueryValue<int>("take", 20);
            var query = request.QueryValue<string?>("query", null);
            var res = await _svc.ListPagedAsync(subjectId, termId, skip, take, query, request.HttpContext.RequestAborted);
            return new OkObjectResult(res);
        });

}
