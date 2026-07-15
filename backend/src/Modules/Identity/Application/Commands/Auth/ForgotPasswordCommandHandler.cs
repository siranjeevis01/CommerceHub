using MediatR;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Identity.Application.Common.Interfaces;
using CommerceHub.Modules.Identity.Domain.Entities;

namespace CommerceHub.Modules.Identity.Application.Commands.Auth;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Unit>
{
    private readonly IIdentityDbContext _context;

    public ForgotPasswordCommandHandler(IIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
            return Unit.Value;

        var otp = new CommerceHub.Modules.Identity.Domain.Entities.Otp
        {
            Email = request.Email,
            OtpCode = Guid.NewGuid().ToString("N")[..6],
            Type = "PasswordReset",
            DeliveryMethod = "Email",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsUsed = false,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.Otps.Add(otp);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
