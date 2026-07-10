using MediatR;
using CommerceHub.Identity.Application.DTOs;

namespace CommerceHub.Identity.Application.Queries;

public record GetUserProfileQuery : IRequest<UserDto>
{
    public int UserId { get; init; }
}
