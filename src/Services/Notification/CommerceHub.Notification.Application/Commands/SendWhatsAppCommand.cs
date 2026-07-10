using MediatR;
using CommerceHub.Notification.Application.Common.Interfaces;

namespace CommerceHub.Notification.Application.Commands;

public record SendWhatsAppCommand : IRequest<bool>
{
    public string To { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public class SendWhatsAppCommandHandler : IRequestHandler<SendWhatsAppCommand, bool>
{
    private readonly IWhatsAppService _whatsAppService;

    public SendWhatsAppCommandHandler(IWhatsAppService whatsAppService)
    {
        _whatsAppService = whatsAppService;
    }

    public async Task<bool> Handle(SendWhatsAppCommand request, CancellationToken cancellationToken)
    {
        return await _whatsAppService.SendWhatsAppMessageAsync(request.To, request.Message, cancellationToken);
    }
}
