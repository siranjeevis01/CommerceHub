namespace CommerceHub.Modules.Identity.Application.DTOs;

public record UserFilterDto
{
    public string? Search { get; init; }
    public string? UserType { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
