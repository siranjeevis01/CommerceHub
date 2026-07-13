using CommerceHub.Product.Application.Commands;
using FluentValidation;

namespace CommerceHub.Product.Application.Validators;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
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

        When(v => !string.IsNullOrEmpty(v.MainImageUrl), () =>
        {
            RuleFor(v => v.MainImageUrl)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri)
                    && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                .WithMessage("Main image URL must be a valid HTTP or HTTPS URL.")
                .MaximumLength(2000).WithMessage("Main image URL must not exceed 2000 characters.");
        });

        When(v => !string.IsNullOrEmpty(v.GalleryImages), () =>
        {
            RuleFor(v => v.GalleryImages)
                .Must(json =>
                {
                    if (string.IsNullOrEmpty(json)) return true;
                    try
                    {
                        var urls = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);
                        if (urls is null) return false;
                        return urls.All(url => Uri.TryCreate(url, UriKind.Absolute, out var uri)
                            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps));
                    }
                    catch
                    {
                        return false;
                    }
                })
                .WithMessage("Gallery images must be a valid JSON array of HTTP or HTTPS URLs.")
                .MaximumLength(8000).WithMessage("Gallery images JSON must not exceed 8000 characters.");
        });

        When(v => v.Variants is { Count: > 0 }, () =>
        {
            RuleForEach(v => v.Variants).ChildRules(variant =>
            {
                variant.RuleFor(v => v.Name)
                    .NotEmpty().WithMessage("Variant name is required.")
                    .MaximumLength(200);

                variant.RuleFor(v => v.SKU)
                    .NotEmpty().WithMessage("Variant SKU is required.")
                    .MaximumLength(50);

                variant.RuleFor(v => v.Price)
                    .GreaterThan(0).WithMessage("Variant price must be greater than zero.");
            });
        });
    }
}
