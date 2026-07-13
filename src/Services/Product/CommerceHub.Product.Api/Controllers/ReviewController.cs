using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Product.Application.Commands;
using CommerceHub.Product.Application.Queries;

namespace CommerceHub.Product.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/reviews")]
public class ReviewController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReviewController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetProductReviews(int productId)
    {
        var result = await _mediator.Send(new GetProductReviewsQuery(productId));
        return Ok(new { Success = true, Data = result });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReviewCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteReviewCommand(id));
        return Ok(new { Success = true, Message = "Review deleted" });
    }
}
