using MediatR;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Identity.Application.Common.Interfaces;
using CommerceHub.Modules.Identity.Domain.Entities;

namespace CommerceHub.Modules.Identity.Application.Commands.Role;

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand>
{
    private readonly IIdentityDbContext _context;

    public AssignRoleCommandHandler(IIdentityDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync([request.UserId], cancellationToken);
        var role = await _context.Roles.FindAsync([request.RoleId], cancellationToken);

        if (user == null || role == null)
            throw new KeyNotFoundException("User or Role not found");

        var alreadyAssigned = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId, cancellationToken);

        if (!alreadyAssigned)
        {
            _context.UserRoles.Add(new UserRole
            {
                UserId = request.UserId,
                RoleId = request.RoleId
            });
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
