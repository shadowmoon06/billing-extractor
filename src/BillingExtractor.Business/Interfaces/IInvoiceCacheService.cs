using BillingExtractor.Business.Models;

namespace BillingExtractor.Business.Interfaces;

public interface IInvoiceCacheService
{
    Task<InvoiceSummaryDto?> GetSummaryAsync(string invoiceNumber);
    Task<InvoiceDetailDto?> GetDetailAsync(string invoiceNumber);
    Task<IEnumerable<InvoiceSummaryDto>?> GetAllSummariesAsync();
    Task SetSummaryAsync(string invoiceNumber, InvoiceSummaryDto summary);
    Task SetDetailAsync(string invoiceNumber, InvoiceDetailDto detail);
    Task SetAllSummariesAsync(IEnumerable<InvoiceSummaryDto> summaries);
    Task DeleteAsync(string invoiceNumber);
    Task InvalidateAllSummariesAsync();
}
