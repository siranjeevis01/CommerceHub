namespace CommerceHub.Modules.Cms.Application.DTOs;

public record BannerDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Subtitle { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public string? LinkUrl { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
