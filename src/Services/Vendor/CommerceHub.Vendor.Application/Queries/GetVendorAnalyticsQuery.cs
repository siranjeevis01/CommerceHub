using AutoMapper;
using CommerceHub.Vendor.Application.Common.Interfaces;
using CommerceHub.Vendor.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Vendor.Application.Queries;

public record GetVendorAnalyticsQuery : IRequest<VendorAnalyticsDto>
{
    public int VendorId { get; init; }
}

public class GetVendorAnalyticsQueryHandler : IRequestHandler<GetVendorAnalyticsQuery, VendorAnalyticsDto>
{
    private readonly IVendorDbContext _context;
    private readonly IMapper _mapper;

    public GetVendorAnalyticsQueryHandler(IVendorDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<VendorAnalyticsDto> Handle(GetVendorAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var vendor = await _context.Vendors
            .Include(v => v.Payouts)
            .FirstOrDefaultAsync(v => v.Id == request.VendorId && !v.IsDeleted, cancellationToken);

        if (vendor == null)
            throw new KeyNotFoundException($"Vendor with Id {request.VendorId} not found.");

        var dto = _mapper.Map<VendorAnalyticsDto>(vendor);
        dto.TotalPayouts = vendor.Payouts?.Count ?? 0;

        return dto;
    }
}
