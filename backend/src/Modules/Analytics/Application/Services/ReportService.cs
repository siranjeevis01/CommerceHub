using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using CommerceHub.Modules.Order.Application.Common.Interfaces;
using CommerceHub.Modules.Analytics.Application.Interfaces;

namespace CommerceHub.Modules.Analytics.Application.Services;

public class ReportService : IReportService
{
    private readonly IOrderDbContext _orderDbContext;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IOrderDbContext orderDbContext, ILogger<ReportService> logger)
    {
        _orderDbContext = orderDbContext;
        _logger = logger;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<byte[]> GenerateDailyReportAsync(DateTime date)
    {
        var startDate = date.Date;
        var endDate = startDate.AddDays(1);

        var orders = await _orderDbContext.Orders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate)
            .ToListAsync();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Daily Report");

        worksheet.Cells["A1"].Value = $"DAILY REPORT - {date:yyyy-MM-dd}";
        worksheet.Cells["A1:D1"].Merge = true;
        worksheet.Cells["A1"].Style.Font.Size = 16;
        worksheet.Cells["A1"].Style.Font.Bold = true;

        worksheet.Cells["A3"].Value = "Summary";
        worksheet.Cells["A3"].Style.Font.Bold = true;
        worksheet.Cells["A4"].Value = "Total Orders";
        worksheet.Cells["B4"].Value = orders.Count;
        worksheet.Cells["A5"].Value = "Total Revenue";
        worksheet.Cells["B5"].Value = orders.Sum(o => o.TotalAmount);
        worksheet.Cells["A6"].Value = "Average Order Value";
        worksheet.Cells["B6"].Value = orders.Any() ? orders.Average(o => o.TotalAmount) : 0;

        worksheet.Cells["A8"].Value = "Order Details";
        worksheet.Cells["A8"].Style.Font.Bold = true;
        worksheet.Cells["A9"].Value = "Order #";
        worksheet.Cells["B9"].Value = "Customer ID";
        worksheet.Cells["C9"].Value = "Total";
        worksheet.Cells["D9"].Value = "Status";

        var row = 10;
        foreach (var order in orders)
        {
            worksheet.Cells[row, 1].Value = order.OrderNumber;
            worksheet.Cells[row, 2].Value = order.UserId;
            worksheet.Cells[row, 3].Value = order.TotalAmount;
            worksheet.Cells[row, 4].Value = order.OrderStatus;
            row++;
        }

        worksheet.Cells.AutoFitColumns();
        return await package.GetAsByteArrayAsync();
    }

    public async Task<byte[]> GenerateWeeklyReportAsync(DateTime date)
    {
        var weekStart = date.Date.AddDays(-(int)date.DayOfWeek);
        var weekEnd = weekStart.AddDays(7);

        var orders = await _orderDbContext.Orders
            .Where(o => o.CreatedAt >= weekStart && o.CreatedAt < weekEnd)
            .ToListAsync();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Weekly Report");

        worksheet.Cells["A1"].Value = $"WEEKLY REPORT - {weekStart:yyyy-MM-dd} to {weekEnd:yyyy-MM-dd}";
        worksheet.Cells["A1:D1"].Merge = true;
        worksheet.Cells["A1"].Style.Font.Size = 16;
        worksheet.Cells["A1"].Style.Font.Bold = true;

        worksheet.Cells["A3"].Value = "Day";
        worksheet.Cells["B3"].Value = "Orders";
        worksheet.Cells["C3"].Value = "Revenue";

        var row = 4;
        for (var i = 0; i < 7; i++)
        {
            var day = weekStart.AddDays(i);
            var dayOrders = orders.Where(o => o.CreatedAt.Date == day.Date).ToList();
            worksheet.Cells[row, 1].Value = day.ToString("yyyy-MM-dd");
            worksheet.Cells[row, 2].Value = dayOrders.Count;
            worksheet.Cells[row, 3].Value = dayOrders.Sum(o => o.TotalAmount);
            row++;
        }

        worksheet.Cells["A" + row].Value = "TOTAL";
        worksheet.Cells["B" + row].Value = orders.Count;
        worksheet.Cells["C" + row].Value = orders.Sum(o => o.TotalAmount);
        worksheet.Cells["A" + row + ":C" + row].Style.Font.Bold = true;

        worksheet.Cells.AutoFitColumns();
        return await package.GetAsByteArrayAsync();
    }

    public async Task<byte[]> GenerateMonthlyReportAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var orders = await _orderDbContext.Orders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate && o.OrderStatus == "Delivered")
            .ToListAsync();

        var orderItems = await _orderDbContext.OrderItems
            .Where(oi => orders.Select(o => o.Id).Contains(oi.OrderId))
            .ToListAsync();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add($"Monthly Report - {startDate:MMMM yyyy}");

        worksheet.Cells["A1"].Value = $"MONTHLY REPORT - {startDate:MMMM yyyy}";
        worksheet.Cells["A1:E1"].Merge = true;
        worksheet.Cells["A1"].Style.Font.Size = 16;
        worksheet.Cells["A1"].Style.Font.Bold = true;

        worksheet.Cells["A3"].Value = "Week";
        worksheet.Cells["B3"].Value = "Orders";
        worksheet.Cells["C3"].Value = "Revenue";
        worksheet.Cells["D3"].Value = "Average Order";
        worksheet.Cells["E3"].Value = "Commission Earned";

        var weeks = orders
            .GroupBy(o => (o.CreatedAt.Day - 1) / 7 + 1)
            .Select(g => new
            {
                Week = g.Key,
                Count = g.Count(),
                Revenue = g.Sum(o => o.TotalAmount),
                Avg = g.Average(o => o.TotalAmount),
                Commission = orderItems
                    .Where(oi => g.Select(o => o.Id).Contains(oi.OrderId))
                    .Sum(oi => oi.Commission)
            })
            .OrderBy(w => w.Week);

        var row = 4;
        foreach (var week in weeks)
        {
            worksheet.Cells[row, 1].Value = $"Week {week.Week}";
            worksheet.Cells[row, 2].Value = week.Count;
            worksheet.Cells[row, 3].Value = week.Revenue;
            worksheet.Cells[row, 4].Value = week.Avg;
            worksheet.Cells[row, 5].Value = week.Commission;
            row++;
        }

        worksheet.Cells[row, 1].Value = "TOTAL";
        worksheet.Cells[row, 2].Value = weeks.Sum(w => w.Count);
        worksheet.Cells[row, 3].Value = weeks.Sum(w => w.Revenue);
        worksheet.Cells[row, 4].Value = weeks.Average(w => w.Avg);
        worksheet.Cells[row, 5].Value = weeks.Sum(w => w.Commission);
        worksheet.Cells[row, 1, row, 5].Style.Font.Bold = true;

        worksheet.Cells.AutoFitColumns();
        return await package.GetAsByteArrayAsync();
    }

    public async Task<byte[]> GenerateVendorReportAsync(int vendorId, DateTime fromDate, DateTime toDate)
    {
        var orderItems = await _orderDbContext.OrderItems
            .Where(oi => oi.VendorId == vendorId)
            .ToListAsync();

        var orderIds = orderItems.Select(oi => oi.OrderId).Distinct().ToList();
        var orders = await _orderDbContext.Orders
            .Where(o => orderIds.Contains(o.Id) && o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
            .ToListAsync();

        var filteredOrderItems = orderItems
            .Where(oi => orders.Select(o => o.Id).Contains(oi.OrderId))
            .ToList();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Vendor Report");

        worksheet.Cells["A1"].Value = $"Vendor Report - {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}";
        worksheet.Cells["A1:E1"].Merge = true;
        worksheet.Cells["A1"].Style.Font.Size = 16;
        worksheet.Cells["A1"].Style.Font.Bold = true;

        worksheet.Cells["A3"].Value = "Product ID";
        worksheet.Cells["B3"].Value = "Quantity Sold";
        worksheet.Cells["C3"].Value = "Revenue";
        worksheet.Cells["D3"].Value = "Commission";
        worksheet.Cells["E3"].Value = "Earnings";

        var productSummary = filteredOrderItems
            .GroupBy(oi => oi.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Quantity = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.TotalPrice),
                Commission = g.Sum(oi => oi.Commission),
                Earnings = g.Sum(oi => oi.VendorEarning)
            });

        var row = 4;
        foreach (var item in productSummary)
        {
            worksheet.Cells[row, 1].Value = item.ProductId;
            worksheet.Cells[row, 2].Value = item.Quantity;
            worksheet.Cells[row, 3].Value = item.Revenue;
            worksheet.Cells[row, 4].Value = item.Commission;
            worksheet.Cells[row, 5].Value = item.Earnings;
            row++;
        }

        worksheet.Cells["A" + row].Value = "TOTAL";
        worksheet.Cells["B" + row].Value = productSummary.Sum(p => p.Quantity);
        worksheet.Cells["C" + row].Value = productSummary.Sum(p => p.Revenue);
        worksheet.Cells["D" + row].Value = productSummary.Sum(p => p.Commission);
        worksheet.Cells["E" + row].Value = productSummary.Sum(p => p.Earnings);
        worksheet.Cells["A" + row + ":E" + row].Style.Font.Bold = true;

        worksheet.Cells.AutoFitColumns();
        return await package.GetAsByteArrayAsync();
    }
}
