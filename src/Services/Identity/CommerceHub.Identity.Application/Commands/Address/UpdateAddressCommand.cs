using MediatR;
using CommerceHub.Identity.Application.DTOs;

namespace CommerceHub.Identity.Application.Commands.Address;

public record UpdateAddressCommand : IRequest<AddressDto>
{
    public int Id { get; init; }
    public string? AddressLine1 { get; init; }
    public string? AddressLine2 { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public string? AddressType { get; init; }
    public bool? IsDefault { get; init; }
}
