using Application.Common.Errors;
using Application.DTO.Notifications;
using Application.Services.Interfaces;
using System.Net;
using System.Net.Http.Json;

namespace Api.Clients;

public sealed class NotificationServiceClient : INotificationService
{
    private readonly HttpClient _httpClient;

    public NotificationServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
    {
        using var response = await _httpClient.GetAsync("api/health/ready", ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<NotificationResponse>> ListMineAsync(
        int userId,
        CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<List<NotificationResponse>>(
            $"api/notifications/users/{userId}", ct) ?? [];
    }

    public async Task<int> CountUnreadMineAsync(int userId, CancellationToken ct = default)
    {
        var response = await _httpClient.GetFromJsonAsync<UnreadNotificationCountResponse>(
            $"api/notifications/users/{userId}/unread-count", ct);
        return response?.Count ?? 0;
    }

    public async Task MarkAsReadAsync(
        Guid notificationId,
        int userId,
        CancellationToken ct = default)
    {
        using var response = await _httpClient.PutAsync(
            $"api/notifications/users/{userId}/{notificationId}/read",
            content: null,
            ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new AppException(AppErrorCode.NotFound, "Notification not found.");

        response.EnsureSuccessStatusCode();
    }
}
