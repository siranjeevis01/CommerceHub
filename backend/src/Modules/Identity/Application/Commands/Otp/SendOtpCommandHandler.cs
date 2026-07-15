using MediatR;
using CommerceHub.Modules.Identity.Application.Common.Interfaces;
using CommerceHub.Modules.Identity.Domain.Entities;

namespace CommerceHub.Modules.Identity.Application.Commands.Otp;

public class SendOtpCommandHandler : IRequestHandler<SendOtpCommand>
{
    private readonly IIdentityDbContext _context;

    public SendOtpCommandHandler(IIdentityDbContext context)
    {
        _context = context;
    }

    public async Task Handle(SendOtpCommand request, CancellationToken cancellationToken)
    {
        var code = Random.Shared.Next(100000, 999999).ToString();
        var otp = new Domain.Entities.Otp
        {
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            OtpCode = code,
            Type = request.Type ?? "verification",
            DeliveryMethod = request.DeliveryMethod ?? "email",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        _context.Otps.Add(otp);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
