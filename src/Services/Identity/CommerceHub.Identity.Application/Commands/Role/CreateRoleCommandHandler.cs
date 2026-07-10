using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using CommerceHub.Identity.Application.Common.Interfaces;
using CommerceHub.Identity.Application.DTOs;
using CommerceHub.Identity.Domain.Entities;

namespace CommerceHub.Identity.Application.Commands.Role;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleDto>
{
    private readonly IIdentityDbContext _context;
    private readonly IMapper _mapper;

    public CreateRoleCommandHandler(IIdentityDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.Roles.AnyAsync(r => r.Name == request.Name, cancellationToken);
        if (exists)
            throw new InvalidOperationException("Role already exists");

        var role = new Domain.Entities.Role
        {
            Name = request.Name,
            Description = request.Description
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<RoleDto>(role);
    }
}
