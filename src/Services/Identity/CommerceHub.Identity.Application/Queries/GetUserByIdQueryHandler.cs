using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using CommerceHub.Identity.Application.Common.Interfaces;
using CommerceHub.Identity.Application.DTOs;

namespace CommerceHub.Identity.Application.Queries;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IIdentityDbContext _context;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(IIdentityDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == request.Id && !u.IsDeleted, cancellationToken);

        if (user == null)
            throw new InvalidOperationException("User not found.");

        return _mapper.Map<UserDto>(user);
    }
}
