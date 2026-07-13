using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Identity.Infrastructure.Data;
using CommerceHub.Identity.Domain.Entities;
using CommerceHub.Order.Infrastructure.Data;

namespace CommerceHub.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUserController : ControllerBase
{
    private readonly IdentityDbContext _identityDb;
    private readonly OrderDbContext _orderDb;

    public AdminUserController(IdentityDbContext identityDb, OrderDbContext orderDb)
    {
        _identityDb = identityDb;
        _orderDb = orderDb;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search = null,
        [FromQuery] string? role = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _identityDb.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(term) ||
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term) ||
                u.Username.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name == role));
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.Username,
                u.FirstName,
                u.LastName,
                u.UserType,
                u.IsActive,
                u.CreatedAt,
                roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
            })
            .ToListAsync();

        return Ok(new { totalCount, page, pageSize, data = users });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _identityDb.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Where(u => u.Id == id)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.Username,
                u.FirstName,
                u.LastName,
                u.PhoneNumber,
                u.UserType,
                u.IsActive,
                u.EmailConfirmed,
                u.TwoFactorEnabled,
                u.CreatedAt,
                roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
            })
            .FirstOrDefaultAsync();

        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpGet("{id}/orders")]
    public async Task<IActionResult> GetUserOrders(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userExists = await _identityDb.Users.AnyAsync(u => u.Id == id);
        if (!userExists) return NotFound();

        var totalCount = await _orderDb.Orders.CountAsync(o => o.UserId == id);
        var orders = await _orderDb.Orders
            .Where(o => o.UserId == id)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                o.OrderStatus,
                o.TotalAmount,
                o.CreatedAt
            })
            .ToListAsync();

        return Ok(new { totalCount, page, pageSize, data = orders });
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateUser(int id)
    {
        var user = await _identityDb.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _identityDb.SaveChangesAsync();
        return Ok(new { message = "User activated" });
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        var user = await _identityDb.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _identityDb.SaveChangesAsync();
        return Ok(new { message = "User deactivated" });
    }

    [HttpPost("{id}/add-role")]
    public async Task<IActionResult> AddRole(int id, [FromBody] AddRoleRequest request)
    {
        var user = await _identityDb.Users.FindAsync(id);
        if (user == null) return NotFound();

        var role = await _identityDb.Roles.FirstOrDefaultAsync(r => r.Name == request.Role);
        if (role == null) return BadRequest(new { message = "Role not found" });

        var exists = await _identityDb.UserRoles.AnyAsync(ur => ur.UserId == id && ur.RoleId == role.Id);
        if (exists) return BadRequest(new { message = "User already has this role" });

        _identityDb.UserRoles.Add(new UserRole { UserId = id, RoleId = role.Id });
        await _identityDb.SaveChangesAsync();
        return Ok(new { message = $"Role '{request.Role}' added" });
    }

    [HttpPost("{id}/remove-role")]
    public async Task<IActionResult> RemoveRole(int id, [FromBody] AddRoleRequest request)
    {
        var userRole = await _identityDb.UserRoles
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync(ur => ur.UserId == id && ur.Role!.Name == request.Role);

        if (userRole == null) return NotFound(new { message = "Role assignment not found" });

        _identityDb.UserRoles.Remove(userRole);
        await _identityDb.SaveChangesAsync();
        return Ok(new { message = $"Role '{request.Role}' removed" });
    }

    [HttpPost("bulk/activate")]
    public async Task<IActionResult> BulkActivate([FromBody] BulkUserRequest request)
    {
        var users = await _identityDb.Users.Where(u => request.UserIds.Contains(u.Id)).ToListAsync();
        foreach (var user in users)
        {
            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
        }
        await _identityDb.SaveChangesAsync();
        return Ok(new { message = $"{users.Count} users activated" });
    }

    [HttpPost("bulk/deactivate")]
    public async Task<IActionResult> BulkDeactivate([FromBody] BulkUserRequest request)
    {
        var users = await _identityDb.Users.Where(u => request.UserIds.Contains(u.Id)).ToListAsync();
        foreach (var user in users)
        {
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
        }
        await _identityDb.SaveChangesAsync();
        return Ok(new { message = $"{users.Count} users deactivated" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _identityDb.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _identityDb.SaveChangesAsync();
        return Ok(new { message = "User soft deleted" });
    }
}

public class AddRoleRequest
{
    public string Role { get; set; } = string.Empty;
}

public class BulkUserRequest
{
    public List<int> UserIds { get; set; } = new();
}
