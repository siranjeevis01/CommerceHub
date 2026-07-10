using MediatR;
using CommerceHub.Cms.Application.Common.Interfaces;

namespace CommerceHub.Cms.Application.Commands.Coupons;

public record DeleteCouponCommand : IRequest
{
    public int Id { get; init; }
}

public class DeleteCouponCommandHandler : IRequestHandler<DeleteCouponCommand>
{
    private readonly ICmsDbContext _context;

    public DeleteCouponCommandHandler(ICmsDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Coupons.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new KeyNotFoundException($"Coupon with Id {request.Id} not found.");

        _context.Coupons.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
