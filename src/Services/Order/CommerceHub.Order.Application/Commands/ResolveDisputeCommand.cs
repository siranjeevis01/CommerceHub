using MediatR;
using CommerceHub.Order.Application.Common.Interfaces;

namespace CommerceHub.Order.Application.Commands;

public record ResolveDisputeCommand : IRequest
{
    public int DisputeId { get; init; }
}

public class ResolveDisputeCommandHandler : IRequestHandler<ResolveDisputeCommand>
{
    private readonly IOrderDbContext _context;

    public ResolveDisputeCommandHandler(IOrderDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ResolveDisputeCommand request, CancellationToken cancellationToken)
    {
        var dispute = await _context.Disputes.FindAsync(new object[] { request.DisputeId }, cancellationToken);
        if (dispute is null)
            throw new InvalidOperationException($"Dispute with ID {request.DisputeId} not found.");

        dispute.Status = "Resolved";

        await _context.SaveChangesAsync(cancellationToken);
    }
}
