using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using CommerceHub.Identity.Application.Common.Interfaces;
using CommerceHub.Identity.Application.DTOs;

namespace CommerceHub.Identity.Application.Commands.Auth;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IIdentityDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;

    public RefreshTokenCommandHandler(
        IIdentityDbContext context,
        IJwtService jwtService,
        IMapper mapper)
    {
        _context = context;
        _jwtService = jwtService;
        _mapper = mapper;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var userId = await _jwtService.ValidateTokenAsync(request.RefreshToken);
        if (userId == null)
            throw new InvalidOperationException("Invalid or expired refresh token.");

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == int.Parse(userId), cancellationToken);

        if (user == null ||
            user.RefreshToken != request.RefreshToken ||
            user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new InvalidOperationException("Invalid or expired refresh token.");

        var tokenResult = await _jwtService.GenerateTokenAsync(user, cancellationToken);

        user.RefreshToken = tokenResult.RefreshToken;
        user.RefreshTokenExpiry = tokenResult.ExpiresAt;
        await _context.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<AuthResponse>(user);
        return response with
        {
            AccessToken = tokenResult.AccessToken,
            RefreshToken = tokenResult.RefreshToken,
            ExpiresAt = tokenResult.ExpiresAt
        };
    }
}
