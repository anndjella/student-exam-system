using Application.DTO.Notifications;

namespace Application.Services.Interfaces;

public interface INotificationService
{
    Task<bool> IsHealthyAsync(CancellationToken ct = default);
    Task<List<NotificationResponse>> ListMineAsync(int userId, CancellationToken ct = default);
    Task<int> CountUnreadMineAsync(int userId, CancellationToken ct = default);
    Task MarkAsReadAsync(Guid notificationId, int userId, CancellationToken ct = default);
}
