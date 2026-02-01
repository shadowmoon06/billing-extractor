using BillingExtractor.Business.Interfaces;
using BillingExtractor.Business.Models;
using BillingExtractor.Data.Entities;
using BillingExtractor.Data.Repositories.SqlRepositories.Interfaces;

namespace BillingExtractor.Business.Services;

public class InvoiceService(IInvoiceRepository invoiceRepository) : IInvoiceService
{
    public async Task<InvoiceDetailDto?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        var invoice = await invoiceRepository.GetByInvoiceNumberAsync(invoiceNumber);
        if (invoice is null)
            return null;

        return new InvoiceDetailDto
        {
            InvoiceNumber = invoice.InvoiceNumber,
            IssuedDate = invoice.IssuedDate,
            VendorName = invoice.VendorName,
            TotalAmount = invoice.TotalAmount,
            LastEdited = invoice.UpdatedAt ?? invoice.CreatedAt,
            Items = [.. invoice.Items.Select(item => new InvoiceItemDto
            {
                ItemId = item.ItemId,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Unit = item.Unit,
                Amount = item.Amount
            })],
            Adjustments = [.. invoice.Adjustments.Select(adj => new InvoiceAdjustmentDto
            {
                Description = adj.Description,
                Amount = adj.Amount
            })]
        };
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
            LastEdited = i.UpdatedAt ?? i.CreatedAt
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
