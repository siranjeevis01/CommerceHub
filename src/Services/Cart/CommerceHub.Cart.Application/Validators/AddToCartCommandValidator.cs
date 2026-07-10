using CommerceHub.Cart.Application.Commands;
using FluentValidation;

namespace CommerceHub.Cart.Application.Validators;

public class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.CartKey)
            .NotEmpty();

        RuleFor(x => x.ProductId)
            .GreaterThan(0);

        RuleFor(x => x.ProductName)
            .NotEmpty();

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}
