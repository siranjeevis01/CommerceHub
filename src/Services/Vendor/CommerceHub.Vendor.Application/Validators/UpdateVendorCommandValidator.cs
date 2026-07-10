using CommerceHub.Vendor.Application.Commands;
using FluentValidation;

namespace CommerceHub.Vendor.Application.Validators;

public class UpdateVendorCommandValidator : AbstractValidator<UpdateVendorCommand>
{
    public UpdateVendorCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0);

        RuleFor(v => v.StoreName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(v => v.BusinessEmail)
            .EmailAddress()
            .When(v => !string.IsNullOrWhiteSpace(v.BusinessEmail));
    }
}
