using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CommerceHub.Modules.Payment.Application.Interfaces;

namespace CommerceHub.Modules.Payment.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(ILogger<InvoiceService> logger)
    {
        _logger = logger;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateInvoiceAsync(InvoiceRequest request)
    {
        return await Task.Run(() =>
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("COMMERCEHUB INVOICE")
                        .FontSize(24)
                        .Bold()
                        .FontColor(Colors.Blue.Darken4)
                        .AlignCenter();

                    page.Content()
                        .PaddingVertical(2, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Item()
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Text($"Invoice #: INV-{request.OrderNumber}")
                                        .FontSize(14)
                                        .Bold();

                                    row.RelativeItem()
                                        .Text($"Date: {DateTime.Now:yyyy-MM-dd HH:mm}")
                                        .FontSize(12)
                                        .AlignRight();
                                });

                            column.Item().LineHorizontal(1);

                            column.Item()
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Column(c =>
                                        {
                                            c.Item().Text("Bill To:").FontSize(12).Bold();
                                            c.Item().Text($"{request.Customer.FirstName} {request.Customer.LastName}".Trim());
                                            c.Item().Text(request.Customer.Email);
                                            c.Item().Text(request.Customer.PhoneNumber);
                                        });

                                    row.RelativeItem()
                                        .Column(c =>
                                        {
                                            c.Item().Text("Shipping Address:").FontSize(12).Bold();
                                            if (request.ShippingAddress != null)
                                            {
                                                c.Item().Text(request.ShippingAddress.AddressLine1);
                                                c.Item().Text($"{request.ShippingAddress.City}, {request.ShippingAddress.State}");
                                                c.Item().Text($"{request.ShippingAddress.PostalCode}, {request.ShippingAddress.Country}");
                                            }
                                        });
                                });

                            column.Item().LineHorizontal(1);

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(50);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("#").Bold();
                                        header.Cell().Text("Product").Bold();
                                        header.Cell().Text("Qty").Bold().AlignRight();
                                        header.Cell().Text("Price").Bold().AlignRight();
                                        header.Cell().Text("Total").Bold().AlignRight();
                                        header.Cell().BorderBottom(1).BorderColor(Colors.Black);
                                    });

                                    var index = 1;
                                    foreach (var item in request.Items)
                                    {
                                        table.Cell().Text(index.ToString());
                                        table.Cell().Text(item.ProductName);
                                        table.Cell().Text(item.Quantity.ToString()).AlignRight();
                                        table.Cell().Text($"₹{item.UnitPrice:F2}").AlignRight();
                                        table.Cell().Text($"₹{item.TotalPrice:F2}").AlignRight();
                                        index++;
                                    }
                                });

                            column.Item().LineHorizontal(1);

                            column.Item()
                                .Row(row =>
                                {
                                    row.RelativeItem();

                                    row.RelativeItem()
                                        .Column(c =>
                                        {
                                            c.Item().Row(r =>
                                            {
                                                r.RelativeItem().Text("Subtotal:");
                                                r.RelativeItem().Text($"₹{request.Subtotal:F2}").AlignRight();
                                            });
                                            c.Item().Row(r =>
                                            {
                                                r.RelativeItem().Text("Shipping:");
                                                r.RelativeItem().Text($"₹{request.ShippingCost:F2}").AlignRight();
                                            });
                                            c.Item().Row(r =>
                                            {
                                                r.RelativeItem().Text("Tax:");
                                                r.RelativeItem().Text($"₹{request.TaxAmount:F2}").AlignRight();
                                            });
                                            c.Item().Row(r =>
                                            {
                                                r.RelativeItem().Text("Discount:");
                                                r.RelativeItem().Text($"-₹{request.DiscountAmount:F2}").AlignRight();
                                            });
                                            c.Item().LineHorizontal(1);
                                            c.Item().Row(r =>
                                            {
                                                r.RelativeItem().Text("TOTAL:").FontSize(14).Bold();
                                                r.RelativeItem().Text($"₹{request.TotalAmount:F2}").FontSize(14).Bold().AlignRight();
                                            });
                                        });
                                });

                            column.Item()
                                .PaddingTop(10)
                                .Text($"Payment Status: {request.PaymentStatus}")
                                .Bold();

                            column.Item()
                                .PaddingTop(20)
                                .Text("Thank you for your business!")
                                .FontSize(12)
                                .FontColor(Colors.Grey.Medium)
                                .AlignCenter();
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generated by CommerceHub • ");
                            x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        });
                });
            });

            return document.GeneratePdf();
        });
    }
}
