using MediatR;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Identity.Application.Common.Interfaces;

namespace CommerceHub.Identity.Application.Commands.Auth;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IIdentityDbContext _context;

    public LogoutCommandHandler(IIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.RefreshToken))
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        return Unit.Value;
    }
}
