using MediatR;
using CommerceHub.Analytics.Application.DTOs;

namespace CommerceHub.Analytics.Application.Queries;

public record GetTopProductsQuery : IRequest<List<TopProductDto>>
{
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public int Count { get; init; } = 10;
}
