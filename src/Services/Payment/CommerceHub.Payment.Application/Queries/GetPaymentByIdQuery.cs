using MediatR;
using CommerceHub.Payment.Application.DTOs;

namespace CommerceHub.Payment.Application.Queries;

public record GetPaymentByIdQuery : IRequest<PaymentDto?>
{
    public int PaymentId { get; init; }
}
