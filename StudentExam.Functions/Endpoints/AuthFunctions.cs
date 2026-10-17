using Application.Auth;
using Application.DTO.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using StudentExam.Functions.Common;

namespace StudentExam.Functions.Endpoints;

public sealed class AuthFunctions(AuthService _auth, FunctionEndpoint endpoint)
{
    [Function("Auth_Login")]
    public Task<IActionResult> Login(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/auth/login")]
        HttpRequest request)
        => endpoint.RunAsync<LoginRequest>(request, EndpointAccess.Anonymous, async (req, user) =>
        {
            var token = await _auth.LoginAsync(req.Username, req.Password, request.HttpContext.RequestAborted);
            return new OkObjectResult(token);
        });

    [Function("Auth_ChangePassword")]
    public Task<IActionResult> ChangePassword(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/auth/change-password")]
        HttpRequest request)
        => endpoint.RunAsync<ChangePasswordRequest>(request, EndpointAccess.Authenticated(), async (req, user) =>
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            await _auth.ChangePasswordAsync(userId, req.CurrentPassword, req.NewPassword, request.HttpContext.RequestAborted);
            return new NoContentResult();
        }, ruleSet: "ChangePassword");

}
