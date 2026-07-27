using Api.Common;
using Application.DTO.Notifications;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Me;

[ApiController]
[Route("api/me/notifications")]
[Authorize(Roles = "Student,Teacher")]
[Authorize(Policy = "PasswordChanged")]
public sealed class NotificationsMeController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsMeController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<NotificationResponse>>> ListMine(CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized();

        return Ok(await _notificationService.ListMineAsync(userId, ct));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<UnreadNotificationCountResponse>> CountUnread(CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized();

        var count = await _notificationService.CountUnreadMineAsync(userId, ct);
        return Ok(new UnreadNotificationCountResponse
        {
            Count = count
        });
    }

    [HttpPut("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized();

        await _notificationService.MarkAsReadAsync(notificationId, userId, ct);
        return NoContent();
    }
}
