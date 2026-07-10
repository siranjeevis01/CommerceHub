using FluentValidation;
using CommerceHub.Cms.Application.Commands.Banners;

namespace CommerceHub.Cms.Application.Validators;

public class CreateBannerCommandValidator : AbstractValidator<CreateBannerCommand>
{
    public CreateBannerCommandValidator()
    {
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(v => v.ImageUrl)
            .NotEmpty().WithMessage("Image URL is required.")
            .MaximumLength(1000).WithMessage("Image URL must not exceed 1000 characters.");

        RuleFor(v => v.LinkUrl)
            .MaximumLength(1000).When(v => v.LinkUrl is not null)
            .WithMessage("Link URL must not exceed 1000 characters.");

        RuleFor(v => v.Subtitle)
            .MaximumLength(500).When(v => v.Subtitle is not null)
            .WithMessage("Subtitle must not exceed 500 characters.");

        RuleFor(v => v.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order must be non-negative.");

        When(v => v.StartDate.HasValue && v.EndDate.HasValue, () =>
        {
            RuleFor(v => v.EndDate)
                .GreaterThan(v => v.StartDate!.Value)
                .WithMessage("End date must be after start date.");
        });
    }
}
