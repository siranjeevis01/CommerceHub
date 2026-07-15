using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using CommerceHub.Modules.Identity.Application.Common.Interfaces;
using CommerceHub.Modules.Identity.Application.DTOs;

namespace CommerceHub.Modules.Identity.Application.Queries;

public class GetAddressesByUserQueryHandler : IRequestHandler<GetAddressesByUserQuery, List<AddressDto>>
{
    private readonly IIdentityDbContext _context;
    private readonly IMapper _mapper;

    public GetAddressesByUserQueryHandler(IIdentityDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<AddressDto>> Handle(GetAddressesByUserQuery request, CancellationToken cancellationToken)
    {
        var addresses = await _context.Addresses
            .Where(a => a.UserId == request.UserId && !a.IsDeleted)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<AddressDto>>(addresses);
    }
}
