using MediatR;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Notification.Application.Commands;
using CommerceHub.Notification.Application.Queries;

namespace CommerceHub.Notification.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserNotifications(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = new GetUserNotificationsQuery { UserId = userId, Page = page, PageSize = pageSize };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("users/{userId}/unread-count")]
    public async Task<IActionResult> GetUnreadCount(int userId)
    {
        var query = new GetUnreadCountQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetNotificationByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        if (result is null)
            return NotFound(new { Success = false, Message = "Notification not found" });
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("send/email")]
    public async Task<IActionResult> SendEmail([FromBody] SendEmailCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Success = result, Data = result });
    }

    [HttpPost("send/sms")]
    public async Task<IActionResult> SendSms([FromBody] SendSmsCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Success = result, Data = result });
    }

    [HttpPost("send/push")]
    public async Task<IActionResult> SendPush([FromBody] SendPushNotificationCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Success = true, Message = "Push notification sent" });
    }

    [HttpPost("send/whatsapp")]
    public async Task<IActionResult> SendWhatsApp([FromBody] SendWhatsAppCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Success = result, Data = result });
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var command = new MarkNotificationReadCommand { Id = id };
        await _mediator.Send(command);
        return Ok(new { Success = true, Message = "Notification marked as read" });
    }
}
