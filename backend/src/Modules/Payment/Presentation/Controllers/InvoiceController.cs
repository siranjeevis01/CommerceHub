using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Modules.Payment.Application.Interfaces;

namespace CommerceHub.Modules.Payment.Presentation.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/invoices")]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoiceController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpPost("generate")]
    [Authorize]
    public async Task<IActionResult> GenerateInvoice([FromBody] InvoiceRequest request)
    {
        var pdfBytes = await _invoiceService.GenerateInvoiceAsync(request);
        return File(pdfBytes, "application/pdf", $"Invoice_{request.OrderNumber}.pdf");
    }

    [HttpPost("generate/{orderId}")]
    [Authorize]
    public async Task<IActionResult> GenerateInvoiceForOrder(int orderId, [FromServices] IMediator mediator)
    {
        return Ok(new { Success = true, Message = "Use POST /api/v1/invoices/generate with InvoiceRequest body" });
    }
}
