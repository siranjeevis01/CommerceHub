using MediatR;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Payment.Application.Commands;
using CommerceHub.Payment.Application.Queries;

namespace CommerceHub.Payment.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/payments")]
public class PaymentController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPaymentById(int id)
    {
        var query = new GetPaymentByIdQuery { PaymentId = id };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetPaymentsByOrder(int orderId)
    {
        var query = new GetPaymentsByOrderQuery { OrderId = orderId };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("methods")]
    public async Task<IActionResult> GetPaymentMethods([FromQuery] int userId)
    {
        var query = new GetPaymentMethodsQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("{id}/refund")]
    public async Task<IActionResult> RefundPayment(int id, [FromBody] RefundPaymentCommand command)
    {
        command = command with { PaymentId = id };
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }
}
