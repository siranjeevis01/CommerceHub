using AutoMapper;
using CommerceHub.Vendor.Application.Common.Interfaces;
using CommerceHub.Vendor.Application.DTOs;
using MediatR;

namespace CommerceHub.Vendor.Application.Commands;

public record ApproveVendorCommand : IRequest<VendorDto>
{
    public int Id { get; init; }
}

public class ApproveVendorCommandHandler : IRequestHandler<ApproveVendorCommand, VendorDto>
{
    private readonly IVendorDbContext _context;
    private readonly IMapper _mapper;

    public ApproveVendorCommandHandler(IVendorDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<VendorDto> Handle(ApproveVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _context.Vendors.FindAsync([request.Id], cancellationToken);
        if (vendor == null)
            throw new KeyNotFoundException($"Vendor with Id {request.Id} not found.");

        vendor.VerificationStatus = "Approved";
        vendor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<VendorDto>(vendor);
    }
}
