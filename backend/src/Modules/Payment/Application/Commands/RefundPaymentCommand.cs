using MediatR;
using CommerceHub.Modules.Payment.Application.Common.Models;

namespace CommerceHub.Modules.Payment.Application.Commands;

public record RefundPaymentCommand : IRequest<RefundResult>
{
    public int PaymentId { get; init; }
    public decimal Amount { get; init; }
    public string Reason { get; init; } = string.Empty;
}
