using BillingExtractor.Data.Entities;

namespace BillingExtractor.Business.Interfaces;

public interface IInvoiceService
{
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);
    Task<IEnumerable<Invoice>> GetAllAsync();
    Task<Invoice> CreateAsync(Invoice invoice);
    Task<bool> DeleteAsync(string invoiceNumber);
}
