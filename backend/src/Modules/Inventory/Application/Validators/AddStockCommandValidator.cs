using CommerceHub.Modules.Inventory.Application.Commands;
using FluentValidation;

namespace CommerceHub.Modules.Inventory.Application.Validators;

public class AddStockCommandValidator : AbstractValidator<AddStockCommand>
{
    public AddStockCommandValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
