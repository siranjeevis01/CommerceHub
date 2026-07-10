using MediatR;
using CommerceHub.Payment.Application.DTOs;

namespace CommerceHub.Payment.Application.Queries;

public record GetAllCouponsQuery : IRequest<IReadOnlyList<CouponDto>>;
