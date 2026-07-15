using FluentValidation;
using CommerceHub.Modules.Notification.Application.Commands;

namespace CommerceHub.Modules.Notification.Application.Validators;

public class SendSmsCommandValidator : AbstractValidator<SendSmsCommand>
{
    public SendSmsCommandValidator()
    {
        RuleFor(x => x.To)
            .NotEmpty().WithMessage("Recipient phone number is required.")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format. Use E.164 format (e.g., +1234567890).");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("SMS message is required.")
            .MaximumLength(160).WithMessage("SMS message cannot exceed 160 characters.");
    }
}
