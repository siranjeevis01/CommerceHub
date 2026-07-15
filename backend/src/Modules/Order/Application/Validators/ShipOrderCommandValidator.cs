using FluentValidation;
using CommerceHub.Modules.Order.Application.Commands;

namespace CommerceHub.Modules.Order.Application.Validators;

public class ShipOrderCommandValidator : AbstractValidator<ShipOrderCommand>
{
    public ShipOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0);

        RuleFor(x => x.TrackingNumber)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Carrier)
            .MaximumLength(100)
            .When(x => x.Carrier is not null);
    }
}
