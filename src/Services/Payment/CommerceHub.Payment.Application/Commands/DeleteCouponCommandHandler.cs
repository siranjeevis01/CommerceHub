using MediatR;
using CommerceHub.Payment.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Payment.Application.Commands;

public class DeleteCouponCommandHandler : IRequestHandler<DeleteCouponCommand>
{
    private readonly IPaymentDbContext _context;

    public DeleteCouponCommandHandler(IPaymentDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException("Coupon not found");

        coupon.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
