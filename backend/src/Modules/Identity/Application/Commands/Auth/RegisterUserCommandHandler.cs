using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using CommerceHub.Modules.Identity.Application.Common.Interfaces;
using CommerceHub.Modules.Identity.Application.DTOs;
using CommerceHub.Modules.Identity.Domain.Entities;
using CommerceHub.Shared.Contracts.Events;

namespace CommerceHub.Modules.Identity.Application.Commands.Auth;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IIdentityDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public RegisterUserCommandHandler(
        IIdentityDbContext context,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IMapper mapper,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<AuthResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser != null)
            throw new InvalidOperationException("A user with this email already exists.");

        var user = new Domain.Entities.User
        {
            Email = request.Email,
            Username = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = _passwordHasher.Hash(request.Password),
            PasswordSalt = string.Empty,
            EmailConfirmed = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new UserRegistered
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.UserType,
            RegisteredAt = user.CreatedAt
        }, cancellationToken);

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
