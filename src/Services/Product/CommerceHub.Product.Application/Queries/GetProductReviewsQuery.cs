using CommerceHub.Product.Application.DTOs;
using MediatR;

namespace CommerceHub.Product.Application.Queries;

public record GetProductReviewsQuery(int ProductId) : IRequest<List<ReviewDto>>;
