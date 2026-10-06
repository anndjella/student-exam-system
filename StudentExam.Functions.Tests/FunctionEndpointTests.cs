using System.Text;
using System.Text.Json;
using Application.Auth;
using Application.Common.Errors;
using Domain.Entity;
using Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using StudentExam.Functions.Common;

namespace StudentExam.Functions.Tests;

public sealed class FunctionEndpointTests
{
    private readonly IConfiguration _cfg = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "test-signing-key-that-is-long-enough-for-hs256",
            ["Jwt:Issuer"] = "StudentExamSystem",
            ["Jwt:Audience"] = "StudentExamSystem"
        })
        .Build();

    private readonly FunctionEndpoint _endpoint;

    public FunctionEndpointTests()
    {
        var services = new ServiceCollection()
            .AddScoped<IValidator<SampleBody>, SampleBodyValidator>()
            .BuildServiceProvider();

        _endpoint = new FunctionEndpoint(
            new JwtAuthenticator(_cfg),
            services,
            NullLogger<FunctionEndpoint>.Instance);
    }

    private static Task<IActionResult> Ok(System.Security.Claims.ClaimsPrincipal _)
        => Task.FromResult<IActionResult>(new OkResult());

    // tokens are made by the real TokenService, so this also proves both sides agree on claims
    private string Token(UserRole role, bool mustChangePassword = false)
        => new TokenService(_cfg).CreateToken(new User(role, "user", "hash", personId: 42)
        {
            ID = 7,
            MustChangePassword = mustChangePassword
        });

    private static HttpRequest Request(string? token = null, string? body = null)
    {
        var ctx = new DefaultHttpContext();
        if (token is not null)
            ctx.Request.Headers.Authorization = $"Bearer {token}";
        if (body is not null)
        {
            var bytes = Encoding.UTF8.GetBytes(body);
            ctx.Request.Body = new MemoryStream(bytes);
            ctx.Request.ContentLength = bytes.Length;
            ctx.Request.ContentType = "application/json";
        }
        return ctx.Request;
    }

    private static int? Status(IActionResult result) => result switch
    {
        IStatusCodeActionResult s => s.StatusCode,
        _ => null
    };

    [Fact]
    public async Task Anonymous_endpoint_runs_without_token()
    {
        var result = await _endpoint.RunAsync(Request(), EndpointAccess.Anonymous, Ok);

        Assert.Equal(200, Status(result));
    }

    [Fact]
    public async Task Missing_token_returns_401_with_bearer_challenge()
    {
        var req = Request();

        var result = await _endpoint.RunAsync(req, EndpointAccess.Authenticated(), Ok);

        Assert.Equal(401, Status(result));
        Assert.Equal("Bearer", req.HttpContext.Response.Headers.WWWAuthenticate.ToString());
    }

    [Fact]
    public async Task Invalid_token_returns_401()
    {
        var result = await _endpoint.RunAsync(Request("not-a-jwt"), EndpointAccess.Authenticated(), Ok);

        Assert.Equal(401, Status(result));
    }

    [Fact]
    public async Task Valid_token_exposes_claims_to_the_action()
    {
        System.Security.Claims.ClaimsPrincipal? seen = null;

        var result = await _endpoint.RunAsync(
            Request(Token(UserRole.Teacher)),
            EndpointAccess.Authenticated("Teacher"),
            user => { seen = user; return Ok(user); });

        Assert.Equal(200, Status(result));
        Assert.True(seen!.TryGetUserId(out var userId));
        Assert.Equal(7, userId);
        Assert.Equal(42, seen.GetPersonId());
        Assert.Equal("Teacher", seen.GetRole());
    }

    [Fact]
    public async Task Wrong_role_returns_403()
    {
        var result = await _endpoint.RunAsync(
            Request(Token(UserRole.Student)),
            EndpointAccess.Authenticated("StudentService", "Teacher"),
            Ok);

        Assert.Equal(403, Status(result));
        var body = Assert.IsType<ContentResult>(result).Content!;
        Assert.Contains("You do not have permission", body);
    }

    [Fact]
    public async Task Password_not_changed_returns_403_with_change_password_detail()
    {
        var result = await _endpoint.RunAsync(
            Request(Token(UserRole.Student, mustChangePassword: true)),
            EndpointAccess.PasswordChanged("Student"),
            Ok);

        Assert.Equal(403, Status(result));
        var body = Assert.IsType<ContentResult>(result).Content!;
        Assert.Contains("You must change your password", body);
    }

    [Fact]
    public async Task Password_changed_policy_passes_when_password_was_changed()
    {
        var result = await _endpoint.RunAsync(
            Request(Token(UserRole.Student)),
            EndpointAccess.PasswordChanged("Student"),
            Ok);

        Assert.Equal(200, Status(result));
    }

    [Fact]
    public async Task Valid_body_reaches_the_action()
    {
        SampleBody? seen = null;

        var result = await _endpoint.RunAsync<SampleBody>(
            Request(body: """{"name":"Ana"}"""),
            EndpointAccess.Anonymous,
            (body, _) => { seen = body; return Task.FromResult<IActionResult>(new OkResult()); },
            ruleSet: "Create");

        Assert.Equal(200, Status(result));
        Assert.Equal("Ana", seen!.Name);
    }

    [Fact]
    public async Task Rule_set_is_applied_and_failures_return_400_validation_problem()
    {
        var result = await _endpoint.RunAsync<SampleBody>(
            Request(body: """{"name":""}"""),
            EndpointAccess.Anonymous,
            (_, _) => throw new InvalidOperationException("action must not run"),
            ruleSet: "Create");

        Assert.Equal(400, Status(result));
        var problem = Assert.IsType<ValidationProblemDetails>(Assert.IsType<ObjectResult>(result).Value);
        Assert.True(problem.Errors.ContainsKey("Name"));
    }

    [Fact]
    public async Task Rules_outside_the_requested_rule_set_are_not_run()
    {
        // empty name only fails the "Create" rule set
        var result = await _endpoint.RunAsync<SampleBody>(
            Request(body: """{"name":""}"""),
            EndpointAccess.Anonymous,
            (_, _) => Task.FromResult<IActionResult>(new OkResult()),
            ruleSet: "Update");

        Assert.Equal(200, Status(result));
    }

    [Fact]
    public async Task Malformed_json_returns_400()
    {
        var result = await _endpoint.RunAsync<SampleBody>(
            Request(body: "{ not json"),
            EndpointAccess.Anonymous,
            (_, _) => Task.FromResult<IActionResult>(new OkResult()));

        Assert.Equal(400, Status(result));
    }

    [Fact]
    public async Task Body_is_not_read_before_authorization()
    {
        var result = await _endpoint.RunAsync<SampleBody>(
            Request(body: "{ not json"),
            EndpointAccess.Authenticated(),
            (_, _) => Task.FromResult<IActionResult>(new OkResult()));

        Assert.Equal(401, Status(result));
    }

    [Theory]
    [InlineData(AppErrorCode.NotFound, 404)]
    [InlineData(AppErrorCode.Validation, 422)]
    [InlineData(AppErrorCode.Conflict, 409)]
    [InlineData(AppErrorCode.Forbidden, 403)]
    [InlineData(AppErrorCode.BadRequest, 400)]
    public async Task AppException_is_mapped_to_problem_details(AppErrorCode code, int expected)
    {
        var result = await _endpoint.RunAsync(
            Request(),
            EndpointAccess.Anonymous,
            _ => throw new AppException(code, "boom"));

        var content = Assert.IsType<ContentResult>(result);
        Assert.Equal(expected, content.StatusCode);
        Assert.Equal("application/problem+json", content.ContentType);

        using var json = JsonDocument.Parse(content.Content!);
        Assert.Equal(expected, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("boom", json.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Unexpected_exception_returns_500_without_leaking_details()
    {
        var result = await _endpoint.RunAsync(
            Request(),
            EndpointAccess.Anonymous,
            _ => throw new InvalidOperationException("secret internals"));

        var content = Assert.IsType<ContentResult>(result);
        Assert.Equal(500, content.StatusCode);
        Assert.DoesNotContain("secret internals", content.Content);
    }

    public sealed class SampleBody
    {
        public string Name { get; set; } = "";
    }

    private sealed class SampleBodyValidator : AbstractValidator<SampleBody>
    {
        public SampleBodyValidator()
        {
            RuleSet("Create", () => RuleFor(x => x.Name).NotEmpty());
        }
    }
}
