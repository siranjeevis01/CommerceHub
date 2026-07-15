using MediatR;
using CommerceHub.Modules.Identity.Application.DTOs;

namespace CommerceHub.Modules.Identity.Application.Queries;

public record GetUserProfileQuery : IRequest<UserDto>
{
    public int UserId { get; init; }
}
