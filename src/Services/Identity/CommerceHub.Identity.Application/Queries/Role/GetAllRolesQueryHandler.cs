using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using CommerceHub.Identity.Application.Common.Interfaces;
using CommerceHub.Identity.Application.DTOs;

namespace CommerceHub.Identity.Application.Queries.Role;

public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, List<RoleDto>>
{
    private readonly IIdentityDbContext _context;
    private readonly IMapper _mapper;

    public GetAllRolesQueryHandler(IIdentityDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _context.Roles
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<RoleDto>>(roles);
    }
}
