using AutoMapper;
using CommerceHub.Modules.Vendor.Application.Common.Interfaces;
using CommerceHub.Modules.Vendor.Application.DTOs;
using MediatR;

namespace CommerceHub.Modules.Vendor.Application.Commands;

public record RejectVendorCommand : IRequest<VendorDto>
{
    public int Id { get; init; }
    public string? Reason { get; init; }
}

public class RejectVendorCommandHandler : IRequestHandler<RejectVendorCommand, VendorDto>
{
    private readonly IVendorDbContext _context;
    private readonly IMapper _mapper;

    public RejectVendorCommandHandler(IVendorDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<VendorDto> Handle(RejectVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _context.Vendors.FindAsync([request.Id], cancellationToken);
        if (vendor == null)
            throw new KeyNotFoundException($"Vendor with Id {request.Id} not found.");

        vendor.VerificationStatus = "Rejected";
        vendor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<VendorDto>(vendor);
    }
}
