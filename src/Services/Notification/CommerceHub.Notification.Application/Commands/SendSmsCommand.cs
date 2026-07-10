using MediatR;
using CommerceHub.Notification.Application.Common.Interfaces;

namespace CommerceHub.Notification.Application.Commands;

public record SendSmsCommand : IRequest<bool>
{
    public string To { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public class SendSmsCommandHandler : IRequestHandler<SendSmsCommand, bool>
{
    private readonly ISmsService _smsService;

    public SendSmsCommandHandler(ISmsService smsService)
    {
        _smsService = smsService;
    }

    public async Task<bool> Handle(SendSmsCommand request, CancellationToken cancellationToken)
    {
        return await _smsService.SendSmsAsync(request.To, request.Message, cancellationToken);
    }
}
