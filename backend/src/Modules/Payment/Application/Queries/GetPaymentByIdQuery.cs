using MediatR;
using CommerceHub.Modules.Payment.Application.DTOs;

namespace CommerceHub.Modules.Payment.Application.Queries;

public record GetPaymentByIdQuery : IRequest<PaymentDto?>
{
    public int PaymentId { get; init; }
}
