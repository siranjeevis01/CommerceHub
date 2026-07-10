using AutoMapper;
using CommerceHub.Vendor.Application.Common.Interfaces;
using CommerceHub.Vendor.Application.DTOs;
using MediatR;

namespace CommerceHub.Vendor.Application.Commands;

public record UpdateVendorCommand : IRequest<VendorDto>
{
    public int Id { get; init; }
    public string StoreName { get; init; } = string.Empty;
    public string? StoreDescription { get; init; }
    public string? BusinessEmail { get; init; }
    public string? BusinessPhone { get; init; }
    public string? BusinessAddress { get; init; }
    public string? GSTNumber { get; init; }
    public string? PANNumber { get; init; }
    public string? BusinessType { get; init; }
}

public class UpdateVendorCommandHandler : IRequestHandler<UpdateVendorCommand, VendorDto>
{
    private readonly IVendorDbContext _context;
    private readonly IMapper _mapper;

    public UpdateVendorCommandHandler(IVendorDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<VendorDto> Handle(UpdateVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _context.Vendors.FindAsync([request.Id], cancellationToken);
        if (vendor == null)
            throw new KeyNotFoundException($"Vendor with Id {request.Id} not found.");

        vendor.StoreName = request.StoreName;
        vendor.StoreDescription = request.StoreDescription;
        vendor.BusinessEmail = request.BusinessEmail;
        vendor.BusinessPhone = request.BusinessPhone;
        vendor.BusinessAddress = request.BusinessAddress;
        vendor.GSTNumber = request.GSTNumber;
        vendor.PANNumber = request.PANNumber;
        vendor.BusinessType = request.BusinessType;
        vendor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<VendorDto>(vendor);
    }
}
