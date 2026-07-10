using AutoMapper;
using CommerceHub.Product.Application.Common.Interfaces;
using CommerceHub.Product.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Product.Application.Queries;

public class GetProductReviewsQueryHandler : IRequestHandler<GetProductReviewsQuery, List<ReviewDto>>
{
    private readonly IProductDbContext _context;
    private readonly IMapper _mapper;

    public GetProductReviewsQueryHandler(IProductDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ReviewDto>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _context.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == request.ProductId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return reviews.Select(r => new ReviewDto
        {
            Id = r.Id,
            Rating = r.Rating,
            Comment = r.Comment,
            Images = r.Images,
            IsVerifiedPurchase = r.IsVerifiedPurchase,
            ProductId = r.ProductId,
            UserId = r.UserId,
            CreatedAt = r.CreatedAt
        }).ToList();
    }
}
