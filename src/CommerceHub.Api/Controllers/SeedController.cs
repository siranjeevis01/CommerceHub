using CommerceHub.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommerceHub.Api.Controllers;

[ApiController]
[Route("api/v1/seed")]
[AllowAnonymous]
public class SeedController : ControllerBase
{
    private readonly SeedDataService _seedService;
    private readonly ILogger<SeedController> _logger;

    public SeedController(SeedDataService seedService, ILogger<SeedController> logger)
    {
        _seedService = seedService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(SeedResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SeedResult), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Seed(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Seed endpoint invoked");

        var result = await _seedService.SeedAsync(cancellationToken);

        if (result.AlreadySeeded)
            return Conflict(result);

        if (result.Errors.Count > 0)
            return Ok(result);

        return Ok(result);
    }
}
