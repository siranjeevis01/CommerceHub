using MediatR;
using CommerceHub.Modules.Payment.Application.DTOs;

namespace CommerceHub.Modules.Payment.Application.Queries;

public record GetAllCouponsQuery : IRequest<IReadOnlyList<CouponDto>>;
