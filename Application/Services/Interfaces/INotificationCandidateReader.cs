using Application.DTO.Notifications;

namespace Application.Services.Interfaces;

public interface INotificationCandidateReader
{
    Task<IReadOnlyList<RegistrationReminderCandidateResponse>> ListRegistrationRemindersAsync(
        DateOnly registrationEndsOn,
        CancellationToken ct = default);

    Task<IReadOnlyList<MissingExamResultCandidateResponse>> ListMissingExamResultsAsync(
        DateOnly examDate,
        CancellationToken ct = default);
}
