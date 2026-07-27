namespace Application.DTO.Notifications;

public sealed class RegistrationReminderCandidateResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = "";
    public string RecipientName { get; set; } = "";
    public int TermId { get; set; }
    public string TermName { get; set; } = "";
    public DateOnly RegistrationEndDate { get; set; }
    public IReadOnlyList<string> SubjectNames { get; set; } = [];
}
