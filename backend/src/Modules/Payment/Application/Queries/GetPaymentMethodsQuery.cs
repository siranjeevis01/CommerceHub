using MediatR;
using CommerceHub.Modules.Payment.Application.DTOs;

namespace CommerceHub.Modules.Payment.Application.Queries;

public record GetPaymentMethodsQuery : IRequest<IReadOnlyList<PaymentMethodDto>>
{
    public int UserId { get; init; }
}
