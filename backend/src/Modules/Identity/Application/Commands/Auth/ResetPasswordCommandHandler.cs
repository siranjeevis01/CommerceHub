using MediatR;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Identity.Application.Common.Interfaces;

namespace CommerceHub.Modules.Identity.Application.Commands.Auth;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IIdentityDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(IIdentityDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
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

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.PasswordSalt = string.Empty;
        user.UpdatedAt = DateTime.UtcNow;

        otp.IsUsed = true;
        otp.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
