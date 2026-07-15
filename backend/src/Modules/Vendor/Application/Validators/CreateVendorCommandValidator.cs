using CommerceHub.Modules.Vendor.Application.Commands;
using FluentValidation;

namespace CommerceHub.Modules.Vendor.Application.Validators;

public class CreateVendorCommandValidator : AbstractValidator<CreateVendorCommand>
{
    public CreateVendorCommandValidator()
    {
        RuleFor(v => v.StoreName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(v => v.BusinessEmail)
            .EmailAddress()
            .When(v => !string.IsNullOrWhiteSpace(v.BusinessEmail));

        RuleFor(v => v.UserId)
            .GreaterThan(0);
    }
}
