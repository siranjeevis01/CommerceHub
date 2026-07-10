using MediatR;
using CommerceHub.Analytics.Application.DTOs;

namespace CommerceHub.Analytics.Application.Queries;

public record GetSalesReportQuery : IRequest<SalesReportDto>
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }
}
