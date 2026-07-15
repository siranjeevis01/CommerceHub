using MediatR;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Modules.Payment.Application.Commands;
using CommerceHub.Modules.Payment.Application.Queries;
using CommerceHub.Modules.Payment.Infrastructure.Services;

namespace CommerceHub.Modules.Payment.Presentation.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/payments")]
public class PaymentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly WhatsAppPaymentService _whatsAppPaymentService;

    public PaymentController(IMediator mediator, WhatsAppPaymentService whatsAppPaymentService)
    {
        _mediator = mediator;
        _whatsAppPaymentService = whatsAppPaymentService;
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

    [HttpPost("upi/qr")]
    public async Task<IActionResult> GenerateUpiQr([FromBody] GenerateUpiQrRequest request)
    {
        var command = new ProcessPaymentCommand
        {
            OrderId = request.OrderId,
            UserId = request.UserId,
            Amount = request.Amount,
            Currency = request.Currency ?? "INR",
            Provider = "upi"
        };
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("whatsapp/send-qr")]
    public async Task<IActionResult> SendWhatsAppQr([FromBody] SendWhatsAppQrRequest request)
    {
        var sent = await _whatsAppPaymentService.SendUpiQrCodeAsync(
            request.PhoneNumber,
            request.UpiUri,
            request.Amount,
            request.Currency ?? "INR",
            request.OrderId);

        return Ok(new { Success = sent, Message = sent ? "QR code sent via WhatsApp" : "Failed to send" });
    }

    [HttpPost("whatsapp/order-confirmation")]
    public async Task<IActionResult> SendOrderConfirmation([FromBody] SendWhatsAppNotificationRequest request)
    {
        var sent = await _whatsAppPaymentService.SendOrderConfirmationAsync(
            request.PhoneNumber,
            request.OrderId,
            request.Amount);

        return Ok(new { Success = sent });
    }

    [HttpPost("whatsapp/shipping-update")]
    public async Task<IActionResult> SendShippingUpdate([FromBody] SendWhatsAppShippingRequest request)
    {
        var sent = await _whatsAppPaymentService.SendShippingUpdateAsync(
            request.PhoneNumber,
            request.OrderId,
            request.Status);

        return Ok(new { Success = sent });
    }

    [HttpPost("whatsapp/share-product")]
    public IActionResult ShareProduct([FromBody] ShareProductRequest request)
    {
        var link = _whatsAppPaymentService.GenerateOrderPaymentLink(
            request.PhoneNumber,
            request.ProductUrl,
            request.ProductPrice,
            request.UpiId);
        return Ok(new { Success = true, Data = new { ShareUrl = link } });
    }

    [HttpPost("whatsapp/share-order")]
    public IActionResult ShareOrder([FromBody] ShareOrderRequest request)
    {
        var link = _whatsAppPaymentService.GenerateOrderPaymentLink(
            request.PhoneNumber,
            request.OrderId,
            request.Amount,
            request.UpiId,
            request.MerchantName);
        return Ok(new { Success = true, Data = new { ShareUrl = link } });
    }
}

public record GenerateUpiQrRequest
{
    public int OrderId { get; init; }
    public int UserId { get; init; }
    public decimal Amount { get; init; }
    public string? Currency { get; init; }
}

public record SendWhatsAppQrRequest
{
    public string PhoneNumber { get; init; } = string.Empty;
    public string UpiUri { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string? Currency { get; init; }
    public string OrderId { get; init; } = string.Empty;
}

public record SendWhatsAppNotificationRequest
{
    public string PhoneNumber { get; init; } = string.Empty;
    public string OrderId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}

public record SendWhatsAppShippingRequest
{
    public string PhoneNumber { get; init; } = string.Empty;
    public string OrderId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}

public record ShareProductRequest
{
    public string ProductName { get; init; } = string.Empty;
    public decimal ProductPrice { get; init; }
    public string ProductUrl { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string UpiId { get; init; } = string.Empty;
}

public record ShareOrderRequest
{
    public string OrderId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public string UpiId { get; init; } = string.Empty;
    public string MerchantName { get; init; } = "CommerceHub";
}
