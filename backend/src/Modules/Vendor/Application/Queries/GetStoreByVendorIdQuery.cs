using AutoMapper;
using CommerceHub.Modules.Vendor.Application.Common.Interfaces;
using CommerceHub.Modules.Vendor.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Modules.Vendor.Application.Queries;

public record GetStoreByVendorIdQuery : IRequest<StoreDto>
{
    public int VendorId { get; init; }
}

public class GetStoreByVendorIdQueryHandler : IRequestHandler<GetStoreByVendorIdQuery, StoreDto>
{
    private readonly IVendorDbContext _context;
    private readonly IMapper _mapper;

    public GetStoreByVendorIdQueryHandler(IVendorDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<StoreDto> Handle(GetStoreByVendorIdQuery request, CancellationToken cancellationToken)
    {
        var vendor = await _context.Vendors
            .FirstOrDefaultAsync(v => v.Id == request.VendorId && !v.IsDeleted, cancellationToken);

        if (vendor == null)
            throw new KeyNotFoundException($"Vendor with Id {request.VendorId} not found.");

        return _mapper.Map<StoreDto>(vendor);
    }
}
