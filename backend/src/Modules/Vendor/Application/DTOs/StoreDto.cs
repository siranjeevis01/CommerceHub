namespace CommerceHub.Modules.Vendor.Application.DTOs;

public class StoreDto
{
    public int Id { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string? StoreDescription { get; set; }
    public string? StoreLogo { get; set; }
    public string? StoreBanner { get; set; }
    public string? BusinessPhone { get; set; }
    public string? BusinessEmail { get; set; }
    public string? BusinessAddress { get; set; }
    public string? GSTNumber { get; set; }
    public string? PANNumber { get; set; }
    public string? BusinessType { get; set; }
}
