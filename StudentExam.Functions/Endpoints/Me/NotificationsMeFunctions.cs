using Application.Auth;
using Application.DTO.Notifications;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using StudentExam.Functions.Common;

namespace StudentExam.Functions.Endpoints.Me;

public sealed class NotificationsMeFunctions(INotificationService _notificationService, FunctionEndpoint endpoint)
{
    [Function("NotificationsMe_ListMine")]
    public Task<IActionResult> ListMine(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/me/notifications")]
        HttpRequest request)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("Student", "Teacher"), async user =>
        {
            if (!user.TryGetUserId(out var userId))
                return new UnauthorizedResult();
            
            return new OkObjectResult(await _notificationService.ListMineAsync(userId, request.HttpContext.RequestAborted));
        });

    [Function("NotificationsMe_CountUnread")]
    public Task<IActionResult> CountUnread(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/me/notifications/unread-count")]
        HttpRequest request)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("Student", "Teacher"), async user =>
        {
            if (!user.TryGetUserId(out var userId))
                return new UnauthorizedResult();
            
            var count = await _notificationService.CountUnreadMineAsync(userId, request.HttpContext.RequestAborted);
            return new OkObjectResult(new UnreadNotificationCountResponse
            {
                Count = count
            });
        });

    [Function("NotificationsMe_MarkAsRead")]
    public Task<IActionResult> MarkAsRead(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "api/me/notifications/{notificationId:guid}/read")]
        HttpRequest request, Guid notificationId)
        => endpoint.RunAsync(request, EndpointAccess.PasswordChanged("Student", "Teacher"), async user =>
        {
            if (!user.TryGetUserId(out var userId))
                return new UnauthorizedResult();
            
            await _notificationService.MarkAsReadAsync(notificationId, userId, request.HttpContext.RequestAborted);
            return new NoContentResult();
        });

}
