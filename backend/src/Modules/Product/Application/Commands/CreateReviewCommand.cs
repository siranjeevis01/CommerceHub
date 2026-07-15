using CommerceHub.Modules.Product.Application.DTOs;
using MediatR;

namespace CommerceHub.Modules.Product.Application.Commands;

public record CreateReviewCommand : IRequest<ReviewDto>
{
    public int Rating { get; init; }
    public string? Comment { get; init; }
    public string? Images { get; init; }
    public bool IsVerifiedPurchase { get; init; }
    public int ProductId { get; init; }
    public int UserId { get; init; }
}
