using MediatR;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Identity.Application.Common.Interfaces;

namespace CommerceHub.Identity.Application.Commands.Auth;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Unit>
{
    private readonly IIdentityDbContext _context;

    public VerifyEmailCommandHandler(IIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var otp = await _context.Otps
            .FirstOrDefaultAsync(o =>
                o.Email == request.Email &&
                o.OtpCode == request.OtpCode &&
                !o.IsUsed, cancellationToken);

        if (otp == null || otp.ExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Invalid or expired OTP.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
            throw new InvalidOperationException("User not found.");

        user.EmailConfirmed = true;
        user.UpdatedAt = DateTime.UtcNow;

        otp.IsUsed = true;
        otp.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
