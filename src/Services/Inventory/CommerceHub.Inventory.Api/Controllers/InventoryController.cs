using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Inventory.Application.Queries;
using CommerceHub.Inventory.Application.Commands;

namespace CommerceHub.Inventory.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/inventory")]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("products/{productId}")]
    public async Task<IActionResult> GetStockLevel(int productId, [FromQuery] int? variantId, [FromQuery] int warehouseId)
    {
        var query = new GetStockLevelQuery(productId, variantId, warehouseId);
        var result = await _mediator.Send(query);
        if (result is null)
            return NotFound(new { Success = false, Message = "Stock not found" });
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("products/{productId}/all")]
    public async Task<IActionResult> GetStockByProduct(int productId, [FromQuery] int? variantId)
    {
        var query = new GetStockByProductQuery(productId, variantId);
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [Authorize(Roles = "Admin,Vendor")]
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockProducts([FromQuery] int? threshold)
    {
        var query = new GetLowStockProductsQuery(threshold);
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [Authorize(Roles = "Admin,Vendor")]
    [HttpGet("movements")]
    public async Task<IActionResult> GetStockMovements(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? productId = null,
        [FromQuery] int? warehouseId = null,
        [FromQuery] string? transactionType = null)
    {
        var query = new GetStockMovementsQuery(page, pageSize, productId, warehouseId, transactionType);
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("reserve")]
    public async Task<IActionResult> ReserveStock([FromBody] ReserveStockCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success)
            return BadRequest(new { Success = false, Message = result.ErrorMessage });
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("release")]
    public async Task<IActionResult> ReleaseStock([FromBody] ReleaseStockCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddStock([FromBody] AddStockCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("deduct")]
    public async Task<IActionResult> DeductStock([FromBody] DeductStockCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> TransferStock([FromBody] TransferStockCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success)
            return BadRequest(new { Success = false, Message = result.ErrorMessage });
        return Ok(new { Success = true, Data = result });
    }
}
