using CommerceHub.Modules.Vendor.Domain.Entities;

namespace CommerceHub.Modules.Vendor.Application.Interfaces;

public interface IVendorDocumentService
{
    Task<IEnumerable<VendorDocument>> GetDocumentsAsync(int vendorId, CancellationToken cancellationToken = default);
    Task<VendorDocument> UploadDocumentAsync(int vendorId, string documentType, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<bool> DeleteDocumentAsync(int documentId, CancellationToken cancellationToken = default);
}
