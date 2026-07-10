using MediatR;
using CommerceHub.Identity.Application.DTOs;
using CommerceHub.Shared.Kernel.Pagination;

namespace CommerceHub.Identity.Application.Queries;

public record GetAllUsersQuery : IRequest<PagedResult<UserDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
}
