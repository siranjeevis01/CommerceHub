using MediatR;
using CommerceHub.Modules.Notification.Application.Common.Interfaces;

namespace CommerceHub.Modules.Notification.Application.Commands;

public record SendEmailCommand : IRequest<bool>
{
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public bool IsHtml { get; init; } = true;
}

public class SendEmailCommandHandler : IRequestHandler<SendEmailCommand, bool>
{
    private readonly IEmailService _emailService;

    public SendEmailCommandHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<bool> Handle(SendEmailCommand request, CancellationToken cancellationToken)
    {
        return await _emailService.SendEmailAsync(
            request.To,
            request.Subject,
            request.Body,
            request.IsHtml,
            cancellationToken);
    }
}
