using MediatR;
using CommerceHub.Identity.Application.DTOs;

namespace CommerceHub.Identity.Application.Queries;

public record GetUserByIdQuery : IRequest<UserDto>
{
    public int Id { get; init; }
}
