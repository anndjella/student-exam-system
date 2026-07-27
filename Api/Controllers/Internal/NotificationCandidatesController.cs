using Application.DTO.Notifications;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace Api.Controllers.Internal;

[ApiController]
[AllowAnonymous]
[Route("api/internal/notification-candidates")]
public sealed class NotificationCandidatesController : ControllerBase
{
    private readonly INotificationCandidateReader _candidateReader;
    private readonly IConfiguration _configuration;

    public NotificationCandidatesController(
        INotificationCandidateReader candidateReader,
        IConfiguration configuration)
    {
        _candidateReader = candidateReader;
        _configuration = configuration;
    }

    [HttpGet("registration-deadline")]
    public async Task<ActionResult<IReadOnlyList<RegistrationReminderCandidateResponse>>> RegistrationDeadline(
        [FromQuery] DateOnly registrationEndsOn,
        CancellationToken ct)
    {
        if (!HasValidServiceKey()) return Unauthorized();
        return Ok(await _candidateReader.ListRegistrationRemindersAsync(registrationEndsOn, ct));
    }

    [HttpGet("missing-exam-results")]
    public async Task<ActionResult<IReadOnlyList<MissingExamResultCandidateResponse>>> MissingExamResults(
        [FromQuery] DateOnly examDate,
        CancellationToken ct)
    {
        if (!HasValidServiceKey()) return Unauthorized();
        return Ok(await _candidateReader.ListMissingExamResultsAsync(examDate, ct));
    }

    private bool HasValidServiceKey()
    {
        var expected = _configuration["ServiceAuthentication:ApiKey"];
        var supplied = Request.Headers["X-Internal-Api-Key"].ToString();

        if (string.IsNullOrWhiteSpace(expected) || string.IsNullOrWhiteSpace(supplied))
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(supplied));
    }
}
