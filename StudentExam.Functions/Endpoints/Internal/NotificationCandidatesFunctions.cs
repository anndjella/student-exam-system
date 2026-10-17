using Application.DTO.Notifications;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using StudentExam.Functions.Common;
using Microsoft.Extensions.Configuration;

namespace StudentExam.Functions.Endpoints.Internal;

public sealed class NotificationCandidatesFunctions(INotificationCandidateReader _candidateReader, IConfiguration _configuration, FunctionEndpoint endpoint)
{
    [Function("NotificationCandidates_RegistrationDeadline")]
    public Task<IActionResult> RegistrationDeadline(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/internal/notification-candidates/registration-deadline")]
        HttpRequest request)
        => endpoint.RunAsync(request, EndpointAccess.Anonymous, async user =>
        {
            var registrationEndsOn = request.QueryValue<DateOnly>("registrationEndsOn", default);
            if (!HasValidServiceKey(request)) return new UnauthorizedResult();
            return new OkObjectResult(await _candidateReader.ListRegistrationRemindersAsync(registrationEndsOn, request.HttpContext.RequestAborted));
        });

    [Function("NotificationCandidates_MissingExamResults")]
    public Task<IActionResult> MissingExamResults(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/internal/notification-candidates/missing-exam-results")]
        HttpRequest request)
        => endpoint.RunAsync(request, EndpointAccess.Anonymous, async user =>
        {
            var cutoffDate = request.QueryValue<DateOnly>("cutoffDate", default);
            if (!HasValidServiceKey(request)) return new UnauthorizedResult();
            return new OkObjectResult(await _candidateReader.ListMissingExamResultsAsync(cutoffDate, request.HttpContext.RequestAborted));
        });

    private bool HasValidServiceKey(HttpRequest request)
    {
        var expected = _configuration["ServiceAuthentication:ApiKey"];
        var supplied = request.Headers["X-Internal-Api-Key"].ToString();

        if (string.IsNullOrWhiteSpace(expected) || string.IsNullOrWhiteSpace(supplied))
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(supplied));
    }
}
