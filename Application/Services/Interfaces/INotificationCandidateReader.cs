using Application.DTO.Notifications;

namespace Application.Services.Interfaces;

public interface INotificationCandidateReader
{
    Task<IReadOnlyList<RegistrationReminderCandidateResponse>> ListRegistrationRemindersAsync(
        DateOnly registrationEndsOn,
        CancellationToken ct = default);

    Task<IReadOnlyList<MissingExamResultCandidateResponse>> ListMissingExamResultsAsync(
        DateOnly cutoffDate,
        CancellationToken ct = default);
}
