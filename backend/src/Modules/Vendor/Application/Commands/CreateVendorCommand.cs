using AutoMapper;
using CommerceHub.Modules.Vendor.Application.Common.Interfaces;
using CommerceHub.Modules.Vendor.Application.DTOs;
using CommerceHub.Modules.Vendor.Domain.Entities;
using MassTransit;
using MediatR;

namespace CommerceHub.Modules.Vendor.Application.Commands;

public record CreateVendorCommand : IRequest<int>
{
    public string StoreName { get; init; } = string.Empty;
    public string? StoreDescription { get; init; }
    public string? BusinessEmail { get; init; }
    public string? BusinessPhone { get; init; }
    public string? BusinessAddress { get; init; }
    public string? GSTNumber { get; init; }
    public string? PANNumber { get; init; }
    public string? BusinessType { get; init; }
    public int UserId { get; init; }
    public decimal CommissionRate { get; init; }
}

public record VendorCreatedEvent
{
    public int VendorId { get; init; }
    public string StoreName { get; init; } = string.Empty;
    public int UserId { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class CreateVendorCommandHandler : IRequestHandler<CreateVendorCommand, int>
{
    private readonly IVendorDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateVendorCommandHandler(IVendorDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<int> Handle(CreateVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = new VendorProfile
        {
            StoreName = request.StoreName,
            StoreDescription = request.StoreDescription,
            BusinessEmail = request.BusinessEmail,
            BusinessPhone = request.BusinessPhone,
            BusinessAddress = request.BusinessAddress,
            GSTNumber = request.GSTNumber,
            PANNumber = request.PANNumber,
            BusinessType = request.BusinessType,
            UserId = request.UserId,
            CommissionRate = request.CommissionRate,
            VerificationStatus = "Pending",
            IsActive = true
        };

        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new VendorCreatedEvent
        {
            VendorId = vendor.Id,
            StoreName = vendor.StoreName,
            UserId = vendor.UserId,
            CreatedAt = vendor.CreatedAt
        }, cancellationToken);

        return vendor.Id;
    }
}
