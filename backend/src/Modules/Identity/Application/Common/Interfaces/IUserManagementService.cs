using CommerceHub.Modules.Identity.Application.DTOs;

namespace CommerceHub.Modules.Identity.Application.Common.Interfaces;

public interface IUserManagementService
{
    Task<(bool Success, string? Error, PaginatedResult<UserDto>? Data)> GetUsersAsync(UserFilterDto filter);
    Task<(bool Success, string? Error, UserDto? Data)> GetUserByIdAsync(int id);
    Task<(bool Success, string? Error)> AssignRolesAsync(int userId, List<int> roleIds);
    Task<(bool Success, string? Error)> SuspendUserAsync(int userId, string reason);
    Task<(bool Success, string? Error)> ActivateUserAsync(int userId);
}
