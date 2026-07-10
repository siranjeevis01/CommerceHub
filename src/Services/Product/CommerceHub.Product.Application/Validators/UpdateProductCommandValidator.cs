using CommerceHub.Product.Application.Commands;
using FluentValidation;

namespace CommerceHub.Product.Application.Validators;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0).WithMessage("Valid product Id is required.");

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
    }
}
