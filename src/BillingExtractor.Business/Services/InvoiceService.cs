using BillingExtractor.Business.Interfaces;
using BillingExtractor.Data.Entities;
using BillingExtractor.Data.Repositories;

namespace BillingExtractor.Business.Services;

public class InvoiceService(IInvoiceRepository invoiceRepository) : IInvoiceService
{
    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        return await invoiceRepository.GetByInvoiceNumberAsync(invoiceNumber);
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync()
    {
        return await invoiceRepository.GetAllAsync();
    }

    public async Task<Invoice> CreateAsync(Invoice invoice)
    {
        return await invoiceRepository.CreateAsync(invoice);
    }

    public async Task<bool> DeleteAsync(string invoiceNumber)
    {
        return await invoiceRepository.DeleteAsync(invoiceNumber);
    }
}
