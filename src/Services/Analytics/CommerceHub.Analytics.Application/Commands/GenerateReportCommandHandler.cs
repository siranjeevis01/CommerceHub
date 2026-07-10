using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Analytics.Application.Common.Interfaces;

namespace CommerceHub.Analytics.Application.Commands;

public class GenerateReportCommandHandler : IRequestHandler<GenerateReportCommand, byte[]>
{
    private readonly IAnalyticsDbContext _dbContext;

    public GenerateReportCommandHandler(IAnalyticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<byte[]> Handle(GenerateReportCommand request, CancellationToken cancellationToken)
    {
        var events = await _dbContext.AnalyticsEvents
            .Where(e => e.Timestamp >= request.From && e.Timestamp <= request.To)
            .ToListAsync(cancellationToken);

        var reportData = new
        {
            ReportType = request.ReportType,
            Period = new { From = request.From, To = request.To },
            TotalEvents = events.Count,
            EventsByType = events.GroupBy(e => e.EventType)
                .Select(g => new { EventType = g.Key, Count = g.Count() }),
            GeneratedAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(reportData, new JsonSerializerOptions { WriteIndented = true });

        return request.Format switch
        {
            ReportFormat.Excel => GenerateExcelReport(json),
            ReportFormat.Pdf => GeneratePdfReport(json),
            _ => Encoding.UTF8.GetBytes(json)
        };
    }

    private static byte[] GenerateExcelReport(string json)
    {
        return Encoding.UTF8.GetBytes(json);
    }

    private static byte[] GeneratePdfReport(string json)
    {
        return Encoding.UTF8.GetBytes(json);
    }
}
