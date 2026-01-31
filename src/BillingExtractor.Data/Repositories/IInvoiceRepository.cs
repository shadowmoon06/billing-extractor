using BillingExtractor.Data.Entities;

namespace BillingExtractor.Data.Repositories;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);
    Task<IEnumerable<Invoice>> GetAllAsync();
    Task<Invoice> CreateAsync(Invoice invoice);
    Task<bool> DeleteAsync(string invoiceNumber);
}
