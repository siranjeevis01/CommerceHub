using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using CommerceHub.Identity.Application.Common.Interfaces;
using CommerceHub.Identity.Application.DTOs;

namespace CommerceHub.Identity.Application.Commands.Address;

public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, AddressDto>
{
    private readonly IIdentityDbContext _context;
    private readonly IMapper _mapper;

    public UpdateAddressCommandHandler(IIdentityDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<AddressDto> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == request.Id && !a.IsDeleted, cancellationToken);

        if (address == null)
            throw new InvalidOperationException("Address not found.");

        if (request.AddressLine1 != null)
            address.AddressLine1 = request.AddressLine1;

        if (request.AddressLine2 != null)
            address.AddressLine2 = request.AddressLine2;

        if (request.City != null)
            address.City = request.City;

        if (request.State != null)
            address.State = request.State;

        if (request.PostalCode != null)
            address.PostalCode = request.PostalCode;

        if (request.Country != null)
            address.Country = request.Country;

        if (request.AddressType != null)
            address.AddressType = request.AddressType;

        if (request.IsDefault.HasValue && request.IsDefault.Value && !address.IsDefault)
        {
            var existingDefaults = await _context.Addresses
                .Where(a => a.UserId == address.UserId && a.IsDefault && a.Id != address.Id && !a.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var addr in existingDefaults)
                addr.IsDefault = false;

            address.IsDefault = true;
        }
        else if (request.IsDefault.HasValue && !request.IsDefault.Value)
        {
            address.IsDefault = false;
        }

        address.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AddressDto>(address);
    }
}
