using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using CommerceHub.Modules.Ai.Application.Commands.Chat;

namespace CommerceHub.Modules.Ai.Presentation.Hubs;

[Authorize]
public class AIHub : Hub
{
    private readonly IMediator _mediator;

    public AIHub(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task SendMessage(string message, int? conversationId = null)
    {
        var userId = int.Parse(Context.UserIdentifier ?? "0");
        var command = new SendMessageCommand
        {
            UserId = userId,
            Message = message,
            ConversationId = conversationId
        };

        var result = await _mediator.Send(command);
        await Clients.Caller.SendAsync("MessageReceived", result);
    }

    public async Task StartConversation(string title, string? initialMessage = null)
    {
        var userId = int.Parse(Context.UserIdentifier ?? "0");
        var command = new CreateConversationCommand
        {
            UserId = userId,
            Title = title,
            InitialMessage = initialMessage
        };

        var result = await _mediator.Send(command);
        await Clients.Caller.SendAsync("ConversationCreated", result);
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        await Clients.Caller.SendAsync("Connected", new { Message = "Connected to AI Agent" });
    }
}
