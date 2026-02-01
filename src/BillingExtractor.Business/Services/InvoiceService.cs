using BillingExtractor.Business.Interfaces;
using BillingExtractor.Business.Models;
using BillingExtractor.Data.Entities;
using BillingExtractor.Data.Repositories.SqlRepositories.Interfaces;

namespace BillingExtractor.Business.Services;

public class InvoiceService(
    IInvoiceRepository invoiceRepository,
    IInvoiceCacheService cacheRepository) : IInvoiceService
{
    public async Task<InvoiceDetailDto?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        // Try cache first
        var cached = await cacheRepository.GetDetailAsync(invoiceNumber);
        if (cached is not null)
            return cached;

        // Cache miss - fetch from SQL
        var invoice = await invoiceRepository.GetByInvoiceNumberAsync(invoiceNumber);
        if (invoice is null)
            return null;

        var detail = MapToDetailDto(invoice);

        // Store both summary and detail in cache
        await Task.WhenAll(
            cacheRepository.SetDetailAsync(invoiceNumber, detail),
            cacheRepository.SetSummaryAsync(invoiceNumber, MapToSummaryDto(invoice))
        );

        return detail;
    }

    public async Task<IEnumerable<InvoiceSummaryDto>> GetAllAsync()
    {
        // Try cache first
        var cached = await cacheRepository.GetAllSummariesAsync();
        if (cached is not null)
            return cached;

        // Cache miss - fetch from SQL
        var invoices = await invoiceRepository.GetAllAsync();
        var summaries = invoices.Select(MapToSummaryDto).ToList();

        // Store in cache
        await cacheRepository.SetAllSummariesAsync(summaries);

        return summaries;
    }

    public async Task<Invoice> CreateAsync(Invoice invoice)
    {
        // Check if a soft-deleted invoice exists with the same invoice number
        var deletedInvoice = await invoiceRepository.GetDeletedByInvoiceNumberAsync(invoice.InvoiceNumber);

        Invoice result;
        if (deletedInvoice is not null)
        {
            // Restore the soft-deleted invoice with new data
            result = await invoiceRepository.RestoreAsync(deletedInvoice, invoice);
        }
        else
        {
            // Create new invoice
            result = await invoiceRepository.CreateAsync(invoice);
        }

        // Store both summary and detail in cache
        var detail = MapToDetailDto(result);
        var summary = MapToSummaryDto(result);

        await Task.WhenAll(
            cacheRepository.SetDetailAsync(result.InvoiceNumber, detail),
            cacheRepository.SetSummaryAsync(result.InvoiceNumber, summary),
            cacheRepository.InvalidateAllSummariesAsync() // Invalidate list cache
        );

        return result;
    }

    public async Task<bool> DeleteAsync(string invoiceNumber)
    {
        var deleted = await invoiceRepository.DeleteAsync(invoiceNumber);

        if (deleted)
        {
            // Remove from cache
            await cacheRepository.DeleteAsync(invoiceNumber);
        }

        return deleted;
    }

    private static InvoiceDetailDto MapToDetailDto(Invoice invoice)
    {
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

    private static InvoiceSummaryDto MapToSummaryDto(Invoice invoice)
    {
        return new InvoiceSummaryDto
        {
            InvoiceNumber = invoice.InvoiceNumber,
            IssuedDate = invoice.IssuedDate,
            VendorName = invoice.VendorName,
            TotalAmount = invoice.TotalAmount,
            LastEdited = invoice.UpdatedAt ?? invoice.CreatedAt
        };
    }
}
