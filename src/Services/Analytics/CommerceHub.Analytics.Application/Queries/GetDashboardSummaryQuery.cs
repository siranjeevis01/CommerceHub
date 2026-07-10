using MediatR;
using CommerceHub.Analytics.Application.DTOs;

namespace CommerceHub.Analytics.Application.Queries;

public record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;
