using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Payment.Application.Common.Interfaces;
using CommerceHub.Payment.Application.DTOs;

namespace CommerceHub.Payment.Application.Queries;

public class GetPaymentsByOrderQueryHandler : IRequestHandler<GetPaymentsByOrderQuery, IReadOnlyList<PaymentDto>>
{
    private readonly IPaymentDbContext _context;
    private readonly IMapper _mapper;

    public GetPaymentsByOrderQueryHandler(IPaymentDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PaymentDto>> Handle(GetPaymentsByOrderQuery request, CancellationToken cancellationToken)
    {
        var payments = await _context.Payments
            .AsNoTracking()
            .Where(p => p.OrderId == request.OrderId)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<PaymentDto>>(payments);
    }
}
