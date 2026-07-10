namespace CommerceHub.Identity.Application.Common.Models;

public record PagedRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
