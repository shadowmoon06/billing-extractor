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

    public async Task<Invoice?> GetDeletedByInvoiceNumberAsync(string invoiceNumber)
    {
        return await context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Adjustments)
            .Where(i => i.DeletedAt != null)
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

    public async Task<Invoice> RestoreAsync(Invoice existingInvoice, Invoice newData)
    {
        // Update invoice properties
        existingInvoice.IssuedDate = newData.IssuedDate;
        existingInvoice.VendorName = newData.VendorName;
        existingInvoice.TotalAmount = newData.TotalAmount;
        existingInvoice.DeletedAt = null;

        // Replace items and adjustments (owned entities stored as JSON)
        existingInvoice.Items.Clear();
        existingInvoice.Items.AddRange(newData.Items);

        existingInvoice.Adjustments.Clear();
        existingInvoice.Adjustments.AddRange(newData.Adjustments);

        await context.SaveChangesAsync();
        return existingInvoice;
    }
}
