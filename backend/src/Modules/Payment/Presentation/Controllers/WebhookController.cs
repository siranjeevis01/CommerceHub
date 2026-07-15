using MediatR;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Modules.Payment.Application.Commands;

namespace CommerceHub.Modules.Payment.Presentation.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhookController : ControllerBase
{
    private readonly IMediator _mediator;

    public WebhookController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("{provider}")]
    public async Task<IActionResult> HandleWebhook(string provider)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault() ?? Request.Headers["X-Razorpay-Signature"].FirstOrDefault() ?? string.Empty;

        var command = new HandleWebhookCommand
        {
            Provider = provider,
            Payload = payload,
            Signature = signature
        };
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }
}
