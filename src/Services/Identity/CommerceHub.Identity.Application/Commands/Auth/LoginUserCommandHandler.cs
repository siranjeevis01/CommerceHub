using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using CommerceHub.Identity.Application.Common.Interfaces;
using CommerceHub.Identity.Application.DTOs;

namespace CommerceHub.Identity.Application.Commands.Auth;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponse>
{
    private readonly IIdentityDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;

    public LoginUserCommandHandler(
        IIdentityDbContext context,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IMapper mapper)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _mapper = mapper;
    }

    public async Task<AuthResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
            throw new InvalidOperationException("Invalid email or password.");

        if (!user.IsActive || user.IsDeleted)
            throw new InvalidOperationException("Account is deactivated.");

        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            throw new InvalidOperationException("Account is temporarily locked. Try again later.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts = (user.FailedLoginAttempts ?? 0) + 1;
            if (user.FailedLoginAttempts >= 5)
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            await _context.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("Invalid email or password.");
        }

        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;

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
