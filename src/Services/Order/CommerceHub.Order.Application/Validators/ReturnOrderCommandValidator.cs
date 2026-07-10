using FluentValidation;
using CommerceHub.Order.Application.Commands;

namespace CommerceHub.Order.Application.Validators;

public class ReturnOrderCommandValidator : AbstractValidator<ReturnOrderCommand>
{
    public ReturnOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0);

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description is not null);

        RuleFor(x => x.RefundAmount)
            .GreaterThan(0);

        RuleFor(x => x.RefundMethod)
            .MaximumLength(50)
            .When(x => x.RefundMethod is not null);
    }
}
