using AutoMapper;
using CommerceHub.Modules.Vendor.Application.Common.Interfaces;
using CommerceHub.Modules.Vendor.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Modules.Vendor.Application.Queries;

public record GetVendorByUserIdQuery : IRequest<VendorDto>
{
    public int UserId { get; init; }
}

public class GetVendorByUserIdQueryHandler : IRequestHandler<GetVendorByUserIdQuery, VendorDto>
{
    private readonly IVendorDbContext _context;
    private readonly IMapper _mapper;

    public GetVendorByUserIdQueryHandler(IVendorDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<VendorDto> Handle(GetVendorByUserIdQuery request, CancellationToken cancellationToken)
    {
        var vendor = await _context.Vendors
            .FirstOrDefaultAsync(v => v.UserId == request.UserId && !v.IsDeleted, cancellationToken);

        if (vendor == null)
            throw new KeyNotFoundException($"Vendor with UserId {request.UserId} not found.");

        return _mapper.Map<VendorDto>(vendor);
    }
}
