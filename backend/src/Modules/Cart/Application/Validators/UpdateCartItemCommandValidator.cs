using CommerceHub.Modules.Cart.Application.Commands;
using FluentValidation;

namespace CommerceHub.Modules.Cart.Application.Validators;

public class UpdateCartItemCommandValidator : AbstractValidator<UpdateCartItemCommand>
{
    public UpdateCartItemCommandValidator()
    {
        RuleFor(x => x.CartKey)
            .NotEmpty();

        RuleFor(x => x.ProductId)
            .GreaterThan(0);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}
