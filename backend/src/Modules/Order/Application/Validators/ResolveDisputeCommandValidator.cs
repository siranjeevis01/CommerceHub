using FluentValidation;
using CommerceHub.Modules.Order.Application.Commands;

namespace CommerceHub.Modules.Order.Application.Validators;

public class ResolveDisputeCommandValidator : AbstractValidator<ResolveDisputeCommand>
{
    public ResolveDisputeCommandValidator()
    {
        RuleFor(x => x.DisputeId)
            .GreaterThan(0);
    }
}
