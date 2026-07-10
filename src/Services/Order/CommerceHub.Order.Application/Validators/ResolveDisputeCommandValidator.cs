using FluentValidation;
using CommerceHub.Order.Application.Commands;

namespace CommerceHub.Order.Application.Validators;

public class ResolveDisputeCommandValidator : AbstractValidator<ResolveDisputeCommand>
{
    public ResolveDisputeCommandValidator()
    {
        RuleFor(x => x.DisputeId)
            .GreaterThan(0);
    }
}
