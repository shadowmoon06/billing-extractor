using BillingExtractor.Data.Contexts;
using BillingExtractor.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BillingExtractor.Data.Repositories;

public class InvoiceRepository(SqlContext context) : IInvoiceRepository
{
    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        return await context.Invoices.FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync()
    {
        return await context.Invoices.ToListAsync();
    }

    public async Task<Invoice> CreateAsync(Invoice invoice)
    {
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();
        return invoice;
    }

    public async Task<bool> DeleteAsync(string invoiceNumber)
    {
        var invoice = await context.Invoices.FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
        if (invoice is null)
            return false;

        context.Invoices.Remove(invoice);
        await context.SaveChangesAsync();
        return true;
    }
}
