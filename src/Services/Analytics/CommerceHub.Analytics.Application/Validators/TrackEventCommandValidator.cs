using FluentValidation;
using CommerceHub.Analytics.Application.Commands;

namespace CommerceHub.Analytics.Application.Validators;

public class TrackEventCommandValidator : AbstractValidator<TrackEventCommand>
{
    public TrackEventCommandValidator()
    {
        RuleFor(x => x.EventType)
            .NotEmpty().WithMessage("Event type is required.")
            .MaximumLength(100).WithMessage("Event type must not exceed 100 characters.");

        RuleFor(x => x.EventData)
            .MaximumLength(5000).WithMessage("Event data must not exceed 5000 characters.");
    }
}
