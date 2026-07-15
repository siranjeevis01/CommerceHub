using MediatR;
using CommerceHub.Modules.Analytics.Application.DTOs;

namespace CommerceHub.Modules.Analytics.Application.Queries;

public record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;
