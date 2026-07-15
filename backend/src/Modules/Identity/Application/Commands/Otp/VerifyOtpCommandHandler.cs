using MediatR;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Identity.Application.Common.Interfaces;

namespace CommerceHub.Modules.Identity.Application.Commands.Otp;

public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand>
{
    private readonly IIdentityDbContext _context;

    public VerifyOtpCommandHandler(IIdentityDbContext context)
    {
        _context = context;
    }

    public async Task Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var otp = await _context.Otps
            .Where(o => o.OtpCode == request.Code && o.Email == request.Email && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (otp == null)
            throw new InvalidOperationException("Invalid or expired OTP");

        otp.IsUsed = true;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
