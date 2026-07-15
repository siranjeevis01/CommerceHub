using FluentValidation;
using CommerceHub.Modules.Order.Application.Commands;

namespace CommerceHub.Modules.Order.Application.Validators;

public class ConfirmOrderCommandValidator : AbstractValidator<ConfirmOrderCommand>
{
    public ConfirmOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0);
    }
}
