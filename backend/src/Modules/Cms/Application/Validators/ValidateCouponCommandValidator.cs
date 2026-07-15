using FluentValidation;
using CommerceHub.Modules.Cms.Application.Commands.Coupons;

namespace CommerceHub.Modules.Cms.Application.Validators;

public class ValidateCouponCommandValidator : AbstractValidator<ValidateCouponCommand>
{
    public ValidateCouponCommandValidator()
    {
        RuleFor(v => v.Code)
            .NotEmpty().WithMessage("Code is required.");

        RuleFor(v => v.OrderTotal)
            .GreaterThanOrEqualTo(0).WithMessage("Order total must be non-negative.");
    }
}
