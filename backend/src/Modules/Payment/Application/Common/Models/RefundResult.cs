namespace CommerceHub.Modules.Payment.Application.Common.Models;

public record RefundResult
{
    public bool Success { get; init; }
    public string RefundId { get; init; } = string.Empty;
    public decimal AmountRefunded { get; init; }
    public string? ErrorMessage { get; init; }
    public string Status { get; init; } = string.Empty;
}
