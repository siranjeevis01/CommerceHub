using CommerceHub.Product.Application.Commands;
using FluentValidation;

namespace CommerceHub.Product.Application.Validators;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

        RuleFor(v => v.SKU)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters.");

        RuleFor(v => v.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(v => v.CategoryId)
            .GreaterThan(0).WithMessage("Category is required.");

        RuleFor(v => v.VendorId)
            .GreaterThan(0).WithMessage("Vendor is required.");

        RuleFor(v => v.Description)
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters.");

        When(v => v.Variants is { Count: > 0 }, () =>
        {
            RuleForEach(v => v.Variants).ChildRules(variant =>
            {
                variant.RuleFor(v => v.Name)
                    .NotEmpty().WithMessage("Variant name is required.")
                    .MaximumLength(200);

                variant.RuleFor(v => v.SKU)
                    .NotEmpty().WithMessage("Variant SKU is required.")
                    .MaximumLength(50);

                variant.RuleFor(v => v.Price)
                    .GreaterThan(0).WithMessage("Variant price must be greater than zero.");
            });
        });
    }
}
