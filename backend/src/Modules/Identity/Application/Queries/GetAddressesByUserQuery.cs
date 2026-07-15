using MediatR;
using CommerceHub.Modules.Identity.Application.DTOs;

namespace CommerceHub.Modules.Identity.Application.Queries;

public record GetAddressesByUserQuery : IRequest<List<AddressDto>>
{
    public int UserId { get; init; }
}
