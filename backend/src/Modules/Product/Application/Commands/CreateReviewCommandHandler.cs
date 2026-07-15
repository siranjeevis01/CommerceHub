using AutoMapper;
using CommerceHub.Modules.Product.Application.Common.Interfaces;
using CommerceHub.Modules.Product.Application.DTOs;
using CommerceHub.Modules.Product.Domain.Entities;
using MediatR;

namespace CommerceHub.Modules.Product.Application.Commands;

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, ReviewDto>
{
    private readonly IProductDbContext _context;
    private readonly IMapper _mapper;

    public CreateReviewCommandHandler(IProductDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ReviewDto> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = new Review
        {
            Rating = request.Rating,
            Comment = request.Comment,
            Images = request.Images,
            IsVerifiedPurchase = request.IsVerifiedPurchase,
            ProductId = request.ProductId,
            UserId = request.UserId
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync(cancellationToken);

        return new ReviewDto
        {
            Id = review.Id,
            Rating = review.Rating,
            Comment = review.Comment,
            Images = review.Images,
            IsVerifiedPurchase = review.IsVerifiedPurchase,
            ProductId = review.ProductId,
            UserId = review.UserId,
            CreatedAt = review.CreatedAt
        };
    }
}
