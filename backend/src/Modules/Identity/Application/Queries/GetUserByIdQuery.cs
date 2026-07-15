using MediatR;
using CommerceHub.Modules.Identity.Application.DTOs;

namespace CommerceHub.Modules.Identity.Application.Queries;

public record GetUserByIdQuery : IRequest<UserDto>
{
    public int Id { get; init; }
}
