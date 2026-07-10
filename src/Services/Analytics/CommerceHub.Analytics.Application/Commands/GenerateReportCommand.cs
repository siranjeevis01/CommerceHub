using MediatR;

namespace CommerceHub.Analytics.Application.Commands;

public enum ReportFormat
{
    Excel,
    Pdf
}

public record GenerateReportCommand : IRequest<byte[]>
{
    public ReportFormat Format { get; init; }
    public string ReportType { get; init; } = string.Empty;
    public DateTime From { get; init; }
    public DateTime To { get; init; }
    public int? UserId { get; init; }
    public int? VendorId { get; init; }
}
