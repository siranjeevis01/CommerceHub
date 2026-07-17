using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Modules.Identity.Application.Common.Interfaces;

namespace CommerceHub.Modules.Identity.Presentation.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/identity/account")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly IGdprService _gdprService;

    public AccountController(IGdprService gdprService)
    {
        _gdprService = gdprService;
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirst("userId")?.Value ?? "0");

    [HttpPost("export")]
    public async Task<IActionResult> ExportData()
    {
        var userId = GetCurrentUserId();
        var (success, error, data) = await _gdprService.ExportUserDataAsync(userId);
        if (!success) return BadRequest(new { Success = false, Message = error });
        return File(data!, "application/zip", "user-data.zip");
    }

    [HttpDelete("delete-account")]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = GetCurrentUserId();
        var (success, error) = await _gdprService.DeleteUserDataAsync(userId);
        if (!success) return BadRequest(new { Success = false, Message = error });
        return Ok(new { Success = true, Message = "Account deleted successfully" });
    }
}
