using AutoMapper;
using CommerceHub.Modules.Identity.Application.Common.Interfaces;
using CommerceHub.Modules.Identity.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace CommerceHub.Modules.Identity.Application.Services;

public class UserManagementService : IUserManagementService
{
    private readonly IIdentityDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(IIdentityDbContext context, IMapper mapper, ILogger<UserManagementService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error, PaginatedResult<UserDto>? Data)> GetUsersAsync(UserFilterDto filter)
    {
        var query = _context.Users
            .Where(u => !u.IsDeleted);

        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(u =>
                u.Email.Contains(filter.Search) ||
                u.Username.Contains(filter.Search) ||
                u.FirstName.Contains(filter.Search) ||
                u.LastName.Contains(filter.Search));

        if (!string.IsNullOrEmpty(filter.UserType))
            query = query.Where(u => u.UserType == filter.UserType);

        var total = query.Count();
        var users = query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        var dtos = _mapper.Map<IEnumerable<UserDto>>(users);
        var result = new PaginatedResult<UserDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
        return (true, null, result);
    }

    public async Task<(bool Success, string? Error, UserDto? Data)> GetUserByIdAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null || user.IsDeleted)
            return (false, "User not found", null);
        return (true, null, _mapper.Map<UserDto>(user));
    }

    public async Task<(bool Success, string? Error)> AssignRolesAsync(int userId, List<int> roleIds)
    {
        var user = _context.Users
            .Where(u => u.Id == userId)
            .FirstOrDefault();

        if (user == null)
            return (false, "User not found");

        var existingRoles = _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToList();

        foreach (var role in existingRoles)
            _context.UserRoles.Remove(role);

        foreach (var roleId in roleIds.Distinct())
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role != null)
                _context.UserRoles.Add(new Domain.Entities.UserRole { UserId = userId, RoleId = roleId });
        }
        await _context.SaveChangesAsync();
        return (true, "Roles assigned");
    }

    public async Task<(bool Success, string? Error)> SuspendUserAsync(int userId, string reason)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return (false, "User not found");
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return (true, "User suspended");
    }

    public async Task<(bool Success, string? Error)> ActivateUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return (false, "User not found");
        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return (true, "User activated");
    }
}
