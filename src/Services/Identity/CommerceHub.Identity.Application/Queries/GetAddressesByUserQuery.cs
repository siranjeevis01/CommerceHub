using MediatR;
using CommerceHub.Identity.Application.DTOs;

namespace CommerceHub.Identity.Application.Queries;

public record GetAddressesByUserQuery : IRequest<List<AddressDto>>
{
    public int UserId { get; init; }
}
