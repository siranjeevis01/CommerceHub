using MediatR;
using CommerceHub.Modules.Payment.Application.DTOs;

namespace CommerceHub.Modules.Payment.Application.Queries;

public record GetPaymentsByOrderQuery : IRequest<IReadOnlyList<PaymentDto>>
{
    public int OrderId { get; init; }
}
