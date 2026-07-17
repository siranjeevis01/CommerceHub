namespace CommerceHub.Modules.Analytics.Application.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateDailyReportAsync(DateTime date);
    Task<byte[]> GenerateWeeklyReportAsync(DateTime date);
    Task<byte[]> GenerateMonthlyReportAsync(int year, int month);
    Task<byte[]> GenerateVendorReportAsync(int vendorId, DateTime fromDate, DateTime toDate);
}
