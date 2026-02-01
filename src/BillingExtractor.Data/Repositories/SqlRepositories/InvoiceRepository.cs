using BillingExtractor.Data.Contexts;
using BillingExtractor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using BillingExtractor.Data.Repositories.SqlRepositories.Interfaces;

namespace BillingExtractor.Data.Repositories.SqlRepositories;

public class InvoiceRepository(SqlContext context) : IInvoiceRepository
{
    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        return await context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Adjustments)
            .Where(i => i.DeletedAt == null)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync()
    {
        return await context.Invoices
            .Where(i => i.DeletedAt == null)
            .ToListAsync();
    }

    public async Task<Invoice> CreateAsync(Invoice invoice)
    {
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();
        return invoice;
    }

    public async Task<bool> DeleteAsync(string invoiceNumber)
    {
        var invoice = await context.Invoices
            .Where(i => i.DeletedAt == null)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
        if (invoice is null)
            return false;

        invoice.DeletedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }
}
