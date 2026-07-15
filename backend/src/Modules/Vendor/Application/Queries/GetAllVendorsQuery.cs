using AutoMapper;
using CommerceHub.Modules.Vendor.Application.Common.Interfaces;
using CommerceHub.Modules.Vendor.Application.DTOs;
using CommerceHub.Shared.Kernel.Pagination;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Modules.Vendor.Application.Queries;

public record GetAllVendorsQuery : IRequest<PagedResult<VendorListDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public string? Status { get; init; }
}

public class GetAllVendorsQueryHandler : IRequestHandler<GetAllVendorsQuery, PagedResult<VendorListDto>>
{
    private readonly IVendorDbContext _context;
    private readonly IMapper _mapper;

    public GetAllVendorsQueryHandler(IVendorDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<VendorListDto>> Handle(GetAllVendorsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Vendors.Where(v => !v.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            query = query.Where(v => v.StoreName.ToLower().Contains(search)
                || (v.BusinessEmail != null && v.BusinessEmail.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(v => v.VerificationStatus == request.Status);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<VendorListDto>(
            _mapper.Map<List<VendorListDto>>(items),
            totalCount,
            request.Page,
            request.PageSize);
    }
}
