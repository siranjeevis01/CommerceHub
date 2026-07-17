using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Modules.Identity.Application.Common.Interfaces;
using CommerceHub.Modules.Identity.Application.DTOs;

namespace CommerceHub.Modules.Identity.Presentation.Controllers.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/identity/admin/users")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class UserManagementController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public UserManagementController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] UserFilterDto filter)
    {
        var (success, error, data) = await _userManagementService.GetUsersAsync(filter);
        if (!success) return BadRequest(new { Success = false, Message = error });
        return Ok(new { Success = true, Data = data });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var (success, error, data) = await _userManagementService.GetUserByIdAsync(id);
        if (!success) return NotFound(new { Success = false, Message = error });
        return Ok(new { Success = true, Data = data });
    }

    [HttpPost("{id}/roles")]
    public async Task<IActionResult> AssignRoles(int id, [FromBody] List<int> roleIds)
    {
        var (success, error) = await _userManagementService.AssignRolesAsync(id, roleIds);
        if (!success) return BadRequest(new { Success = false, Message = error });
        return Ok(new { Success = true, Message = "Roles assigned" });
    }

    [HttpPost("{id}/suspend")]
    public async Task<IActionResult> SuspendUser(int id, [FromBody] string reason)
    {
        var (success, error) = await _userManagementService.SuspendUserAsync(id, reason);
        if (!success) return BadRequest(new { Success = false, Message = error });
        return Ok(new { Success = true, Message = "User suspended" });
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateUser(int id)
    {
        var (success, error) = await _userManagementService.ActivateUserAsync(id);
        if (!success) return BadRequest(new { Success = false, Message = error });
        return Ok(new { Success = true, Message = "User activated" });
    }
}
