using FluentValidation;
using CommerceHub.Order.Application.Commands;

namespace CommerceHub.Order.Application.Validators;

public class DeliverOrderCommandValidator : AbstractValidator<DeliverOrderCommand>
{
    public DeliverOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0);

        RuleFor(x => x.OtpCode)
            .MaximumLength(20)
            .When(x => x.OtpCode is not null);

        RuleFor(x => x.LocationName)
            .MaximumLength(200)
            .When(x => x.LocationName is not null);
    }
}
