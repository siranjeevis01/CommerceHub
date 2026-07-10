using MediatR;
using CommerceHub.Payment.Application.DTOs;

namespace CommerceHub.Payment.Application.Queries;

public record GetPaymentMethodsQuery : IRequest<IReadOnlyList<PaymentMethodDto>>
{
    public int UserId { get; init; }
}
