namespace Application.DTO.Notifications;

public sealed class MissingExamResultCandidateResponse
{
    public int UserId { get; set; }
    public int TeacherId { get; set; }
    public string Email { get; set; } = "";
    public string RecipientName { get; set; } = "";
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
    public int TermId { get; set; }
    public string TermName { get; set; } = "";
    public DateOnly ExamDate { get; set; }
    public int MissingResultCount { get; set; }
}
