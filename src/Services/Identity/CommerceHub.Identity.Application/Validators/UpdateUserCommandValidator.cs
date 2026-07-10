using FluentValidation;
using CommerceHub.Identity.Application.Commands.User;

namespace CommerceHub.Identity.Application.Validators;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0).WithMessage("A valid user ID is required.");

        RuleFor(v => v.FirstName)
            .MaximumLength(100).When(v => v.FirstName != null)
            .WithMessage("First name must not exceed 100 characters.");

        RuleFor(v => v.LastName)
            .MaximumLength(100).When(v => v.LastName != null)
            .WithMessage("Last name must not exceed 100 characters.");
    }
}
