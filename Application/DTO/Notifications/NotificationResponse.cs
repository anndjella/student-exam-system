using Domain.Enums;

namespace Application.DTO.Notifications;

public sealed class NotificationResponse
{
    public Guid ID { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ReadAtUtc { get; set; }
}
