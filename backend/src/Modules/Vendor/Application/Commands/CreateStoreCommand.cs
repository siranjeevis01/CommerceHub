using AutoMapper;
using CommerceHub.Modules.Vendor.Application.Common.Interfaces;
using CommerceHub.Modules.Vendor.Application.DTOs;
using MediatR;

namespace CommerceHub.Modules.Vendor.Application.Commands;

public record CreateStoreCommand : IRequest<StoreDto>
{
    public int VendorId { get; init; }
    public string StoreName { get; init; } = string.Empty;
    public string? StoreDescription { get; init; }
    public string? StoreLogo { get; init; }
    public string? StoreBanner { get; init; }
    public string? BusinessPhone { get; init; }
    public string? BusinessEmail { get; init; }
    public string? BusinessAddress { get; init; }
    public string? GSTNumber { get; init; }
    public string? PANNumber { get; init; }
    public string? BusinessType { get; init; }
}

public class CreateStoreCommandHandler : IRequestHandler<CreateStoreCommand, StoreDto>
{
    private readonly IVendorDbContext _context;
    private readonly IMapper _mapper;

    public CreateStoreCommandHandler(IVendorDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<StoreDto> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _context.Vendors.FindAsync([request.VendorId], cancellationToken);
        if (vendor == null)
            throw new KeyNotFoundException($"Vendor with Id {request.VendorId} not found.");

        vendor.StoreName = request.StoreName;
        vendor.StoreDescription = request.StoreDescription;
        vendor.StoreLogo = request.StoreLogo;
        vendor.StoreBanner = request.StoreBanner;
        vendor.BusinessPhone = request.BusinessPhone;
        vendor.BusinessEmail = request.BusinessEmail;
        vendor.BusinessAddress = request.BusinessAddress;
        vendor.GSTNumber = request.GSTNumber;
        vendor.PANNumber = request.PANNumber;
        vendor.BusinessType = request.BusinessType;
        vendor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<StoreDto>(vendor);
    }
}
