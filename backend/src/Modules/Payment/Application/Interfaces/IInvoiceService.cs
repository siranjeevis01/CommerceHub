namespace CommerceHub.Modules.Payment.Application.Interfaces;

public interface IInvoiceService
{
    Task<byte[]> GenerateInvoiceAsync(InvoiceRequest request);
}

public record InvoiceRequest
{
    public string OrderNumber { get; init; } = string.Empty;
    public decimal Subtotal { get; init; }
    public decimal ShippingCost { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public string PaymentStatus { get; init; } = string.Empty;
    public InvoiceCustomer Customer { get; init; } = new();
    public InvoiceAddress? ShippingAddress { get; init; }
    public List<InvoiceItem> Items { get; init; } = new();
}

public record InvoiceCustomer
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
}

public record InvoiceAddress
{
    public string AddressLine1 { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
}

public record InvoiceItem
{
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TotalPrice { get; init; }
}
