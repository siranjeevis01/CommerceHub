using AutoMapper;
using CommerceHub.Vendor.Application.Common.Interfaces;
using CommerceHub.Vendor.Application.DTOs;
using CommerceHub.Vendor.Domain.Entities;
using MediatR;

namespace CommerceHub.Vendor.Application.Commands;

public record CreatePayoutCommand : IRequest<PayoutDto>
{
    public int VendorId { get; init; }
    public decimal Amount { get; init; }
    public string? PaymentMethod { get; init; }
}

public class CreatePayoutCommandHandler : IRequestHandler<CreatePayoutCommand, PayoutDto>
{
    private readonly IVendorDbContext _context;
    private readonly IMapper _mapper;

    public CreatePayoutCommandHandler(IVendorDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PayoutDto> Handle(CreatePayoutCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _context.Vendors.FindAsync([request.VendorId], cancellationToken);
        if (vendor == null)
            throw new KeyNotFoundException($"Vendor with Id {request.VendorId} not found.");

        var payout = new VendorPayout
        {
            VendorId = request.VendorId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            Status = "Pending",
            PayoutNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20]
        };

        _context.Payouts.Add(payout);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PayoutDto>(payout);
    }
}
