using BillingExtractor.Data.Entities;

namespace BillingExtractor.Data.Repositories.SqlRepositories.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);
    Task<Invoice?> GetDeletedByInvoiceNumberAsync(string invoiceNumber);
    Task<IEnumerable<Invoice>> GetAllAsync();
    Task<Invoice> CreateAsync(Invoice invoice);
    Task<Invoice> RestoreAsync(Invoice existingInvoice, Invoice newData);
    Task<bool> DeleteAsync(string invoiceNumber);
}
