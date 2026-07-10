using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using CommerceHub.Identity.Application.Common.Interfaces;
using CommerceHub.Identity.Application.DTOs;
using CommerceHub.Identity.Domain.Entities;

namespace CommerceHub.Identity.Application.Commands.Address;

public class AddAddressCommandHandler : IRequestHandler<AddAddressCommand, AddressDto>
{
    private readonly IIdentityDbContext _context;
    private readonly IMapper _mapper;

    public AddAddressCommandHandler(IIdentityDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<AddressDto> Handle(AddAddressCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _context.Users
            .AnyAsync(u => u.Id == request.UserId, cancellationToken);

        if (!userExists)
            throw new InvalidOperationException("User not found.");

        if (request.IsDefault)
        {
            var existingDefaults = await _context.Addresses
                .Where(a => a.UserId == request.UserId && a.IsDefault && !a.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var addr in existingDefaults)
                addr.IsDefault = false;
        }

        var address = new Domain.Entities.Address
        {
            UserId = request.UserId,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            AddressType = request.AddressType,
            IsDefault = request.IsDefault,
            CreatedAt = DateTime.UtcNow
        };

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AddressDto>(address);
    }
}
