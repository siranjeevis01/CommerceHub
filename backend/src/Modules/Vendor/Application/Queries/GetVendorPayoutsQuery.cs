using AutoMapper;
using CommerceHub.Modules.Vendor.Application.Common.Interfaces;
using CommerceHub.Modules.Vendor.Application.DTOs;
using CommerceHub.Shared.Kernel.Pagination;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Modules.Vendor.Application.Queries;

public record GetVendorPayoutsQuery : IRequest<PagedResult<PayoutDto>>
{
    public int VendorId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetVendorPayoutsQueryHandler : IRequestHandler<GetVendorPayoutsQuery, PagedResult<PayoutDto>>
{
    private readonly IVendorDbContext _context;
    private readonly IMapper _mapper;

    public GetVendorPayoutsQueryHandler(IVendorDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<PayoutDto>> Handle(GetVendorPayoutsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Payouts.Where(p => p.VendorId == request.VendorId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<PayoutDto>(
            _mapper.Map<List<PayoutDto>>(items),
            totalCount,
            request.Page,
            request.PageSize);
    }
}
