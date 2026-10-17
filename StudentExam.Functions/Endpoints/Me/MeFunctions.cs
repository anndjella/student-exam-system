using Application.Auth;
using Application.DTO.Me;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using StudentExam.Functions.Common;

namespace StudentExam.Functions.Endpoints.Me;

public sealed class MeFunctions(IMeService _svc, FunctionEndpoint endpoint)
{
    [Function("Me_Get")]
    public Task<IActionResult> Get(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/me")]
        HttpRequest request)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged(), async user =>
        {
            if (!user.TryGetPid(out var personId)) return new UnauthorizedResult();
            
            var resp = await _svc.GetMeAsync(personId, request.HttpContext.RequestAborted);
            return new OkObjectResult(resp);
        });

}
