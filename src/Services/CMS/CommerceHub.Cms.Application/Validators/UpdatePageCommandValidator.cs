using FluentValidation;
using CommerceHub.Cms.Application.Commands.Pages;

namespace CommerceHub.Cms.Application.Validators;

public class UpdatePageCommandValidator : AbstractValidator<UpdatePageCommand>
{
    public UpdatePageCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0).WithMessage("Id must be greater than zero.");

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(v => v.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(200).WithMessage("Slug must not exceed 200 characters.")
            .Matches("^[a-z0-9-]+$").WithMessage("Slug must contain only lowercase letters, numbers, and hyphens.");

        RuleFor(v => v.Content)
            .MaximumLength(50000).When(v => v.Content is not null)
            .WithMessage("Content must not exceed 50000 characters.");

        RuleFor(v => v.MetaTitle)
            .MaximumLength(100).When(v => v.MetaTitle is not null)
            .WithMessage("Meta title must not exceed 100 characters.");

        RuleFor(v => v.MetaDescription)
            .MaximumLength(300).When(v => v.MetaDescription is not null)
            .WithMessage("Meta description must not exceed 300 characters.");
    }
}
