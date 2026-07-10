using MediatR;
using CommerceHub.Payment.Application.DTOs;

namespace CommerceHub.Payment.Application.Queries;

public record GetPaymentsByOrderQuery : IRequest<IReadOnlyList<PaymentDto>>
{
    public int OrderId { get; init; }
}
