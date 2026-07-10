using CommerceHub.Shared.Contracts.Events;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Product.Application.Commands;
using CommerceHub.Product.Application.Queries;

namespace CommerceHub.Product.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/products")]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;

    public ProductController(IMediator mediator, IPublishEndpoint publishEndpoint)
    {
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllProductsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));
        if (result == null) return NotFound();

        var userIdClaim = User.FindFirst("userId");
        await _publishEndpoint.Publish(new ProductViewed
        {
            ProductId = id,
            UserId = userIdClaim is not null ? int.Parse(userIdClaim.Value) : null,
            ViewedAt = DateTime.UtcNow
        });

        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] SearchProductsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("by-category/{categoryId}")]
    public async Task<IActionResult> GetByCategory(int categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetProductsByCategoryQuery(categoryId) { Page = page, PageSize = pageSize });
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("by-vendor/{vendorId}")]
    public async Task<IActionResult> GetByVendor(int vendorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetProductsByVendorQuery(vendorId) { Page = page, PageSize = pageSize });
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductCommand command)
    {
        await _mediator.Send(command with { Id = id });
        return Ok(new { Success = true, Message = "Product updated" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteProductCommand(id));
        return Ok(new { Success = true, Message = "Product deleted" });
    }
}
