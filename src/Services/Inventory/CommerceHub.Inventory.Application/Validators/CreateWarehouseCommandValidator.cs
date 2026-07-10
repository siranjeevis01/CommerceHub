using CommerceHub.Inventory.Application.Commands;
using FluentValidation;

namespace CommerceHub.Inventory.Application.Validators;

public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Code)
            .MaximumLength(50);

        RuleFor(x => x.Address)
            .MaximumLength(500);

        RuleFor(x => x.City)
            .MaximumLength(100);

        RuleFor(x => x.Country)
            .MaximumLength(100);
    }
}
