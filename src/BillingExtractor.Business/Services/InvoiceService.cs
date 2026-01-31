using BillingExtractor.Business.Interfaces;
using BillingExtractor.Business.Models;
using BillingExtractor.Data.Entities;
using BillingExtractor.Data.Repositories.SqlRepositories.Interfaces;

namespace BillingExtractor.Business.Services;

public class InvoiceService(IInvoiceRepository invoiceRepository) : IInvoiceService
{
    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        return await invoiceRepository.GetByInvoiceNumberAsync(invoiceNumber);
    }

    public async Task<IEnumerable<InvoiceSummaryDto>> GetAllAsync()
    {
        var invoices = await invoiceRepository.GetAllAsync();
        return invoices.Select(i => new InvoiceSummaryDto
        {
            InvoiceNumber = i.InvoiceNumber,
            IssuedDate = i.IssuedDate,
            VendorName = i.VendorName,
            TotalAmount = i.TotalAmount,
            LastEdited = i.CreatedAt
        });
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
