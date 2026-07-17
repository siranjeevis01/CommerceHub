using CommerceHub.Modules.Vendor.Application.Common.Interfaces;
using CommerceHub.Modules.Vendor.Application.Interfaces;
using CommerceHub.Modules.Vendor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommerceHub.Modules.Vendor.Application.Services;

public class VendorDocumentService : IVendorDocumentService
{
    private readonly IVendorDbContext _context;
    private readonly ILogger<VendorDocumentService> _logger;

    public VendorDocumentService(IVendorDbContext context, ILogger<VendorDocumentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<VendorDocument>> GetDocumentsAsync(int vendorId, CancellationToken cancellationToken = default)
    {
        return await _context.VendorDocuments
            .Where(d => d.VendorId == vendorId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<VendorDocument> UploadDocumentAsync(
        int vendorId,
        string documentType,
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var vendor = await _context.Vendors.FindAsync([vendorId], cancellationToken);
        if (vendor is null)
            throw new InvalidOperationException($"Vendor with ID {vendorId} not found");

        var uploadDir = Path.Combine("uploads", "vendors", vendorId.ToString(), "documents");
        Directory.CreateDirectory(uploadDir);

        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var filePath = Path.Combine(uploadDir, uniqueFileName);

        await using var fileStreamOutput = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);

        var document = new VendorDocument
        {
            VendorId = vendorId,
            DocumentType = documentType,
            FileUrl = filePath,
            VerificationStatus = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.VendorDocuments.Add(document);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document {DocumentType} uploaded for vendor {VendorId}", documentType, vendorId);
        return document;
    }

    public async Task<bool> DeleteDocumentAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var doc = await _context.VendorDocuments.FindAsync([documentId], cancellationToken);
        if (doc is null)
            return false;

        doc.IsDeleted = true;
        doc.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document {DocumentId} soft-deleted", documentId);
        return true;
    }
}
