using FluentValidation;
using CommerceHub.Notification.Application.Commands;

namespace CommerceHub.Notification.Application.Validators;

public class SendEmailCommandValidator : AbstractValidator<SendEmailCommand>
{
    public SendEmailCommandValidator()
    {
        RuleFor(x => x.To)
            .NotEmpty().WithMessage("Recipient email address is required.")
            .EmailAddress().WithMessage("Invalid email address format.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Email subject is required.")
            .MaximumLength(500).WithMessage("Subject cannot exceed 500 characters.");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Email body is required.");
    }
}
