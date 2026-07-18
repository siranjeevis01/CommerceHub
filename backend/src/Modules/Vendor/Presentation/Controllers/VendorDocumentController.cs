using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Modules.Vendor.Application.Interfaces;

namespace CommerceHub.Modules.Vendor.Presentation.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/vendors/documents")]
[Authorize(Roles = "Vendor,Admin")]
public class VendorDocumentController : ControllerBase
{
    private readonly IVendorDocumentService _documentService;

    public VendorDocumentController(IVendorDocumentService documentService)
    {
        _documentService = documentService;
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirst("userId")?.Value ?? "0");
    }

    [HttpGet]
    public async Task<IActionResult> GetDocuments()
    {
        var userId = GetCurrentUserId();
        var result = await _documentService.GetDocumentsAsync(userId, CancellationToken.None);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UploadDocument(
        [FromForm] string documentType,
        [FromForm] IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { Success = false, Message = "No file uploaded" });

        var userId = GetCurrentUserId();
        using var stream = file.OpenReadStream();
        var result = await _documentService.UploadDocumentAsync(userId, documentType, stream, file.FileName, CancellationToken.None);
        return Ok(new { Success = true, Data = result });
    }

    [HttpDelete("{documentId}")]
    public async Task<IActionResult> DeleteDocument(int documentId)
    {
        var success = await _documentService.DeleteDocumentAsync(documentId, CancellationToken.None);
        if (!success)
            return NotFound(new { Success = false, Message = "Document not found" });

        return Ok(new { Success = true, Message = "Document deleted" });
    }
}
