using BillingExtractor.Business.Models;
using BillingExtractor.Data.Entities;

namespace BillingExtractor.Business.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceDetailDto?> GetByInvoiceNumberAsync(string invoiceNumber);
    Task<IEnumerable<InvoiceSummaryDto>> GetAllAsync();
    Task<Result<Invoice>> CreateAsync(Invoice invoice);
    Task<Result<bool>> DeleteAsync(string invoiceNumber);
}
