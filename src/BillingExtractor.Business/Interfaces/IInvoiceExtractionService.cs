using BillingExtractor.Business.Models;

namespace BillingExtractor.Business.Interfaces;

public interface IInvoiceExtractionService
{
    Task<InvoiceExtractedInfo> ExtractFromImageAsync(byte[] imageBytes, string mimeType);
    Task<InvoiceExtractedInfo> ExtractFromFilePathAsync(string filePath);
}
