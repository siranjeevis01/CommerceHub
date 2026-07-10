using AutoMapper;
using CommerceHub.Vendor.Application.Common.Interfaces;
using CommerceHub.Vendor.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Vendor.Application.Queries;

public record GetVendorByIdQuery : IRequest<VendorDto>
{
    public int Id { get; init; }
}

public class GetVendorByIdQueryHandler : IRequestHandler<GetVendorByIdQuery, VendorDto>
{
    private readonly IVendorDbContext _context;
    private readonly IMapper _mapper;

    public GetVendorByIdQueryHandler(IVendorDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<VendorDto> Handle(GetVendorByIdQuery request, CancellationToken cancellationToken)
    {
        var vendor = await _context.Vendors
            .FirstOrDefaultAsync(v => v.Id == request.Id && !v.IsDeleted, cancellationToken);

        if (vendor == null)
            throw new KeyNotFoundException($"Vendor with Id {request.Id} not found.");

        return _mapper.Map<VendorDto>(vendor);
    }
}
