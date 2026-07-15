using MediatR;
using CommerceHub.Modules.Analytics.Application.DTOs;

namespace CommerceHub.Modules.Analytics.Application.Queries;

public record GetSalesReportQuery : IRequest<SalesReportDto>
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }
}
