using System.Collections.Concurrent;
using BillingExtractor.API.Configurations;
using BillingExtractor.Business.Interfaces;
using BillingExtractor.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace BillingExtractor.API.Controllers;

[EnableRateLimiting("fixed")]
public class InvoiceController(
    IInvoiceService invoiceService,
    IOptions<ImageUploadSettings> imageUploadSettings,
    IInvoiceExtractionService invoiceExtractionService) : BaseController
{
    private readonly ImageUploadSettings _imageSettings = imageUploadSettings.Value;
    private const int MaxConcurrentExtractions = 5;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var invoices = await invoiceService.GetAllAsync();
        return Ok(invoices);
    }

    [HttpGet("{invoiceNumber}")]
    public async Task<IActionResult> GetByInvoiceNumber(string invoiceNumber)
    {
        var invoice = await invoiceService.GetByInvoiceNumberAsync(invoiceNumber);
        if (invoice is null)
            return NotFound();

        return Ok(invoice);
    }

    // [HttpPost]
    // public async Task<IActionResult> Create([FromBody] Invoice invoice)
    // {
    //     var created = await invoiceService.CreateAsync(invoice);
    //     return CreatedAtAction(nameof(GetByInvoiceNumber), new { invoiceNumber = created.InvoiceNumber }, created);
    // }

    [HttpDelete("{invoiceNumber}")]
    public async Task<IActionResult> Delete(string invoiceNumber)
    {
        var deleted = await invoiceService.DeleteAsync(invoiceNumber);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpPost("extract")]
    [EnableRateLimiting("extract")]
    public async Task<IActionResult> ExtractInvoiceInfo(List<IFormFile> images)
    {
        if (images is null || images.Count == 0)
        {
            return BadRequest("No image files provided");
        }

        if (images.Count > _imageSettings.MaxFilesPerRequest)
        {
            return BadRequest($"Maximum {_imageSettings.MaxFilesPerRequest} images allowed per request");
        }

        var errors = new List<string>();
        for (int i = 0; i < images.Count; i++)
        {
            var image = images[i];
            if (!_imageSettings.AllowedMimeTypes.Contains(image.ContentType))
            {
                errors.Add($"File '{image.FileName}' has invalid type. Allowed types: {string.Join(", ", _imageSettings.AllowedMimeTypes)}");
            }
            if (image.Length > _imageSettings.MaxFileSizeBytes)
            {
                errors.Add($"File '{image.FileName}' exceeds maximum allowed size of {_imageSettings.MaxFileSizeBytes / 1024 / 1024}MB");
            }
        }

        if (errors.Count > 0)
        {
            return BadRequest(new { Errors = errors });
        }

        // Extract info from all images with limited concurrency
        var extractionResults = new ConcurrentBag<(string FileName, Business.Models.InvoiceExtractedInfo Info)>();
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = MaxConcurrentExtractions };

        await Parallel.ForEachAsync(images, parallelOptions, async (image, ct) =>
        {
            using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream, ct);
            var imageBytes = memoryStream.ToArray();

            var extractedInfo = await invoiceExtractionService.ExtractFromImageAsync(imageBytes, image.ContentType);
            extractionResults.Add((image.FileName, extractedInfo));
        });

        // Group by invoice number and insert
        var groupedByInvoice = extractionResults
            .GroupBy(r => r.Info.InvoiceNumber ?? string.Empty);

        var savedInvoices = new List<Invoice>();
        var duplicateInvoiceNumbers = new List<string>();
        var amountMismatchWarnings = new List<string>();
        var extractionErrors = new List<string>();

        foreach (var group in groupedByInvoice)
        {
            // Combine data from all pages for this invoice
            var fileNames = group.Select(g => g.FileName).ToList();
            var allInfos = group.Select(g => g.Info).ToList();

            // Get combined metadata (prefer non-null/non-empty values from any page)
            var invoiceNumber = allInfos.FirstOrDefault(i => !string.IsNullOrEmpty(i.InvoiceNumber))?.InvoiceNumber;
            var vendorName = allInfos.FirstOrDefault(i => !string.IsNullOrEmpty(i.VendorName))?.VendorName;
            var issuedDate = allInfos.FirstOrDefault(i => i.IssuedDate is not null)?.IssuedDate;

            // Validate required fields for the grouped invoice
            var missingFields = new List<string>();
            if (string.IsNullOrEmpty(invoiceNumber))
                missingFields.Add("Invoice Number");
            if (string.IsNullOrEmpty(vendorName))
                missingFields.Add("Vendor Name");
            if (issuedDate is null)
                missingFields.Add("Issued Date");

            if (missingFields.Count > 0)
            {
                var filesDescription = string.Join(", ", fileNames.Select(f => $"'{f}'"));
                extractionErrors.Add($"Files [{filesDescription}]: Missing required fields - {string.Join(", ", missingFields)}");
                continue;
            }

            // Check if invoice already exists
            var existingInvoice = await invoiceService.GetByInvoiceNumberAsync(invoiceNumber!);
            if (existingInvoice is not null)
            {
                duplicateInvoiceNumbers.Add(invoiceNumber!);
                continue;
            }

            // Combine items from all pages and calculate totals
            var allItems = group.SelectMany(g => g.Info.Items).ToList();
            var allAdjustments = group.SelectMany(g => g.Info.Adjustments).ToList();

            // Calculate items total (quantity * unit price)
            var itemsTotal = allItems.Sum(item => (item.Quantity ?? 0) * (item.UnitPrice ?? 0));

            // Group adjustments by description and sum amounts
            var groupedAdjustments = allAdjustments
                .GroupBy(adj => adj.Description ?? "Other")
                .Select(g => new InvoiceAdjustment
                {
                    Description = g.Key,
                    Amount = g.Sum(a => a.Amount ?? 0)
                })
                .ToList();

            // Sum all adjustments
            var adjustmentsTotal = groupedAdjustments.Sum(adj => adj.Amount);

            // Total = items + adjustments
            var calculatedTotal = itemsTotal + adjustmentsTotal;

            // Get extracted total from image (sum from all pages for this invoice)
            var extractedTotal = group.Sum(g => g.Info.TotalAmount ?? 0);

            // Compare extracted vs calculated total (allow small rounding difference)
            if (extractedTotal > 0 && Math.Abs(extractedTotal - calculatedTotal) > 0.01m)
            {
                amountMismatchWarnings.Add(
                    $"Invoice '{invoiceNumber}': Extracted total ({extractedTotal:C}) differs from calculated total ({calculatedTotal:C})");
            }

            // Use calculated total for consistency
            var totalAmount = calculatedTotal;

            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber!,
                IssuedDate = issuedDate!.Value.ToUniversalTime(),
                VendorName = vendorName!,
                TotalAmount = totalAmount,
                Items = [.. allItems.Select(item => new InvoiceItem
                {
                    ItemId = item.ItemId ?? string.Empty,
                    Description = item.Description,
                    Quantity = item.Quantity ?? 0,
                    UnitPrice = item.UnitPrice ?? 0,
                    Unit = item.Unit ?? string.Empty,
                    Amount = item.Amount ?? 0
                })],
                Adjustments = groupedAdjustments
            };

            var saved = await invoiceService.CreateAsync(invoice);
            savedInvoices.Add(saved);
        }

        if (extractionErrors.Count > 0)
        {
            return BadRequest(new
            {
                Message = "Failed to extract required information from one or more invoices",
                Errors = extractionErrors
            });
        }

        return Ok(new
        {
            ExtractedCount = extractionResults.Count,
            SavedInvoices = savedInvoices,
            DuplicateInvoiceNumbers = duplicateInvoiceNumbers,
            AmountMismatchWarnings = amountMismatchWarnings
        });
    }
}
