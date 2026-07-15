using MediatR;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Identity.Application.Common.Interfaces;

namespace CommerceHub.Modules.Identity.Application.Commands.Address;

public class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand, Unit>
{
    private readonly IIdentityDbContext _context;

    public DeleteAddressCommandHandler(IIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == request.Id && !a.IsDeleted, cancellationToken);

        if (address == null)
            throw new InvalidOperationException("Address not found.");

        address.IsDeleted = true;
        address.IsActive = false;
        address.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
