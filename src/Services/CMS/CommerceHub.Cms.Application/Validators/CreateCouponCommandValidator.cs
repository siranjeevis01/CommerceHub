using FluentValidation;
using CommerceHub.Cms.Application.Commands.Coupons;

namespace CommerceHub.Cms.Application.Validators;

public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(v => v.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.")
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Code must contain only letters, numbers, underscores, and hyphens.");

        RuleFor(v => v.Type)
            .NotEmpty().WithMessage("Type is required.")
            .Must(t => t == "Percentage" || t == "Fixed")
            .WithMessage("Type must be 'Percentage' or 'Fixed'.");

        When(v => v.Type == "Fixed", () =>
        {
            RuleFor(v => v.DiscountAmount)
                .NotNull().WithMessage("Discount amount is required for Fixed type.")
                .GreaterThan(0).WithMessage("Discount amount must be greater than zero.");
        });

        When(v => v.Type == "Percentage", () =>
        {
            RuleFor(v => v.DiscountPercentage)
                .NotNull().WithMessage("Discount percentage is required for Percentage type.")
                .InclusiveBetween(0, 100).WithMessage("Discount percentage must be between 0 and 100.");
        });

        RuleFor(v => v.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0).When(v => v.MinimumOrderAmount.HasValue)
            .WithMessage("Minimum order amount must be non-negative.");

        RuleFor(v => v.MaxUsageCount)
            .GreaterThan(0).When(v => v.MaxUsageCount.HasValue)
            .WithMessage("Max usage count must be greater than zero.");

        When(v => v.ValidFrom.HasValue && v.ValidTo.HasValue, () =>
        {
            RuleFor(v => v.ValidTo)
                .GreaterThan(v => v.ValidFrom!.Value)
                .WithMessage("Valid to must be after valid from.");
        });
    }
}
