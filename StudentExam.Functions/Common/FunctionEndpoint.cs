using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using Application.Auth;
using FluentValidation;
using Infrastructure.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace StudentExam.Functions.Common;

// Every HTTP function goes through here. It does, in the same order, what the
// Api pipeline did for each controller action:
//   1. JWT authentication            (UseAuthentication + JwtBearer)
//   2. roles / PasswordChanged       (UseAuthorization + [Authorize])
//   3. body binding + FluentValidation ([FromBody] + auto-validation, 400)
//   4. run the action
//   5. exception -> problem+json     (ExceptionHandlingMiddleware)
public sealed class FunctionEndpoint
{
    private const string ValidationProblemType = "https://tools.ietf.org/html/rfc9110#section-15.5.1";

    private readonly JwtAuthenticator _authenticator;
    private readonly IServiceProvider _services;
    private readonly ILogger<FunctionEndpoint> _log;

    public FunctionEndpoint(
        JwtAuthenticator authenticator,
        IServiceProvider services,
        ILogger<FunctionEndpoint> log)
    {
        _authenticator = authenticator;
        _services = services;
        _log = log;
    }

    public Task<IActionResult> RunAsync(
        HttpRequest req,
        EndpointAccess access,
        Func<ClaimsPrincipal, Task<IActionResult>> action)
        => ExecuteAsync(req, access, action);

    // ruleSet = the old [CustomizeValidator(RuleSet = "...")]; null runs the default rules
    public Task<IActionResult> RunAsync<TBody>(
        HttpRequest req,
        EndpointAccess access,
        Func<TBody, ClaimsPrincipal, Task<IActionResult>> action,
        string? ruleSet = null)
        => ExecuteAsync(req, access, async user =>
        {
            var (body, rejected) = await BindAndValidateAsync<TBody>(req, ruleSet);
            return rejected ?? await action(body!, user);
        });

    private async Task<IActionResult> ExecuteAsync(
        HttpRequest req,
        EndpointAccess access,
        Func<ClaimsPrincipal, Task<IActionResult>> action)
    {
        try
        {
            var auth = await _authenticator.AuthenticateAsync(req);
            if (auth.Principal is not null)
                req.HttpContext.User = auth.Principal;

            var denied = Authorize(req, access, auth);
            if (denied is not null)
                return denied;

            return await action(req.HttpContext.User);
        }
        catch (Exception ex)
        {
            return Problem(ex);
        }
    }

    private static IActionResult? Authorize(HttpRequest req, EndpointAccess access, AuthResult auth)
    {
        if (access.AllowAnonymous)
            return null;

        if (auth.Outcome != AuthOutcome.Authenticated)
        {
            // JwtBearer's challenge: 401, empty body, WWW-Authenticate header
            req.HttpContext.Response.Headers.WWWAuthenticate = auth.Outcome == AuthOutcome.InvalidToken
                ? "Bearer error=\"invalid_token\""
                : "Bearer";
            return new UnauthorizedResult();
        }

        var user = auth.Principal!;

        // same rule as MustChangePasswordClearedHandler: only an explicit "false" passes
        var passwordChanged = string.Equals(user.FindFirstValue("mcp"), "false", StringComparison.OrdinalIgnoreCase);
        if (access.RequirePasswordChanged && !passwordChanged)
            return Forbidden(user);

        if (access.Roles.Count > 0 && !access.Roles.Any(user.IsInRole))
            return Forbidden(user);

        return null;
    }

    // same body as the Api's JwtBearerEvents.OnForbidden
    private static IActionResult Forbidden(ClaimsPrincipal user)
        => new ContentResult
        {
            StatusCode = StatusCodes.Status403Forbidden,
            ContentType = "application/json; charset=utf-8",
            Content = JsonSerializer.Serialize(new
            {
                type = "https://errors.yourdomain.com/forbidden",
                title = "Forbidden",
                status = 403,
                detail = JwtValidation.ForbiddenDetail(user.FindFirstValue("mcp") == "true")
            })
        };

    private async Task<(TBody? Body, IActionResult? Rejected)> BindAndValidateAsync<TBody>(
        HttpRequest req,
        string? ruleSet)
    {
        var ct = req.HttpContext.RequestAborted;

        TBody? body;
        try
        {
            body = req.ContentLength == 0
                ? default
                : await JsonSerializer.DeserializeAsync<TBody>(req.Body, JsonSerializerOptions.Web, ct);
        }
        catch (JsonException ex)
        {
            return (default, ValidationProblem(req, new()
            {
                [ex.Path ?? "$"] = [ex.Message]
            }));
        }

        if (body is null)
        {
            return (default, ValidationProblem(req, new()
            {
                [""] = ["A non-empty request body is required."]
            }));
        }

        var validator = _services.GetService<IValidator<TBody>>();
        if (validator is null)
            return (body, null);

        var result = ruleSet is null
            ? await validator.ValidateAsync(body, ct)
            : await validator.ValidateAsync(body, o => o.IncludeRuleSets(ruleSet), ct);

        if (result.IsValid)
            return (body, null);

        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

        return (default, ValidationProblem(req, errors));
    }

    // same shape MVC's [ApiController] returned for invalid input
    private static IActionResult ValidationProblem(HttpRequest req, Dictionary<string, string[]> errors)
    {
        var problem = new ValidationProblemDetails(errors)
        {
            Type = ValidationProblemType,
            Status = StatusCodes.Status400BadRequest
        };
        problem.Extensions["traceId"] = Activity.Current?.Id ?? req.HttpContext.TraceIdentifier;

        return new ObjectResult(problem)
        {
            StatusCode = StatusCodes.Status400BadRequest,
            ContentTypes = { "application/problem+json" }
        };
    }

    private IActionResult Problem(Exception ex)
    {
        // mapping is shared with the Api (Infrastructure/Errors/ExceptionMapper.cs)
        var error = ExceptionMapper.Map(ex);

        var problem = new ProblemDetails
        {
            Status = error.Status,
            Title = error.Title,
            Detail = error.Detail,
            Type = error.Type
        };
        if (error.Extensions is not null)
            foreach (var kv in error.Extensions) problem.Extensions[kv.Key] = kv.Value!;

        _log.Log(error.Level, ex, "{Title} ({Code})", error.Title, error.Code);

        // serialized like the Api middleware did (default options), so the bodies match byte for byte
        return new ContentResult
        {
            StatusCode = error.Status,
            ContentType = "application/problem+json",
            Content = JsonSerializer.Serialize(problem)
        };
    }
}
