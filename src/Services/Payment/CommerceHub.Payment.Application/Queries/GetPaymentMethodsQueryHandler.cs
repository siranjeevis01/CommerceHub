using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Payment.Application.Common.Interfaces;
using CommerceHub.Payment.Application.DTOs;

namespace CommerceHub.Payment.Application.Queries;

public class GetPaymentMethodsQueryHandler : IRequestHandler<GetPaymentMethodsQuery, IReadOnlyList<PaymentMethodDto>>
{
    private readonly IPaymentDbContext _context;
    private readonly IMapper _mapper;

    public GetPaymentMethodsQueryHandler(IPaymentDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PaymentMethodDto>> Handle(GetPaymentMethodsQuery request, CancellationToken cancellationToken)
    {
        var methods = await _context.PaymentMethods
            .AsNoTracking()
            .Where(pm => pm.UserId == request.UserId && pm.IsActive)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<PaymentMethodDto>>(methods);
    }
}
