using AutoMapper;
using CommerceHub.Vendor.Application.Common.Interfaces;
using CommerceHub.Vendor.Application.DTOs;
using MassTransit;
using MediatR;
using CommerceHub.Shared.Contracts.Events;

namespace CommerceHub.Vendor.Application.Commands;

public record ProcessPayoutCommand : IRequest<PayoutDto>
{
    public int Id { get; init; }
    public string? TransactionId { get; init; }
}

public class ProcessPayoutCommandHandler : IRequestHandler<ProcessPayoutCommand, PayoutDto>
{
    private readonly IVendorDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public ProcessPayoutCommandHandler(IVendorDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<PayoutDto> Handle(ProcessPayoutCommand request, CancellationToken cancellationToken)
    {
        var payout = await _context.Payouts.FindAsync([request.Id], cancellationToken);
        if (payout == null)
            throw new KeyNotFoundException($"Payout with Id {request.Id} not found.");

        payout.Status = "Completed";
        payout.TransactionId = request.TransactionId;
        payout.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new VendorPayoutCompleted
        {
            VendorId = payout.VendorId,
            PayoutId = payout.Id,
            PayoutNumber = payout.PayoutNumber,
            Amount = payout.Amount
        }, cancellationToken);

        return _mapper.Map<PayoutDto>(payout);
    }
}
