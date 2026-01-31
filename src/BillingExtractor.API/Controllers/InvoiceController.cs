using BillingExtractor.API.Configurations;
using BillingExtractor.Business.Interfaces;
using BillingExtractor.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BillingExtractor.API.Controllers;

public class InvoiceController(
    IInvoiceService invoiceService,
    IOptions<ImageUploadSettings> imageUploadSettings,
    IInvoiceExtractionService invoiceExtractionService) : BaseController
{
    private readonly ImageUploadSettings _imageSettings = imageUploadSettings.Value;

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
    public async Task<IActionResult> ExtractInvoiceInfo(List<IFormFile> images)
    {
        if (images is null || images.Count == 0)
        {
            return BadRequest("No image files provided");
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

        // Extract info from all images
        var extractionResults = new List<(string FileName, Business.Models.InvoiceExtractedInfo Info)>();
        foreach (var image in images)
        {
            using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            var extractedInfo = await invoiceExtractionService.ExtractFromImageAsync(imageBytes, image.ContentType);
            extractionResults.Add((image.FileName, extractedInfo));
        }

        // Group by invoice number and insert
        var groupedByInvoice = extractionResults
            .Where(r => !string.IsNullOrEmpty(r.Info.InvoiceNumber))
            .GroupBy(r => r.Info.InvoiceNumber!);

        var savedInvoices = new List<Invoice>();
        var duplicateInvoiceNumbers = new List<string>();
        foreach (var group in groupedByInvoice)
        {
            // Take the first extraction result for this invoice number
            var info = group.First().Info;

            // Check if invoice already exists
            var existingInvoice = await invoiceService.GetByInvoiceNumberAsync(info.InvoiceNumber!);
            if (existingInvoice is not null)
            {
                duplicateInvoiceNumbers.Add(info.InvoiceNumber!);
                continue;
            }

            var invoice = new Invoice
            {
                InvoiceNumber = info.InvoiceNumber!,
                IssuedDate = info.IssuedDate?.ToUniversalTime() ?? DateTime.UtcNow,
                VendorName = info.VendorName ?? "Unknown",
                TotalAmount = info.TotalAmount ?? 0,
                Items = [.. info.Items.Select(item => new InvoiceItem
                {
                    ItemId = item.ItemId ?? string.Empty,
                    Description = item.Description,
                    Quantity = item.Quantity ?? 0,
                    UnitPrice = item.UnitPrice ?? 0,
                    Unit = item.Unit ?? string.Empty,
                    Amount = item.Amount ?? 0
                })]
            };

            var saved = await invoiceService.CreateAsync(invoice);
            savedInvoices.Add(saved);
        }

        return Ok(new
        {
            ExtractedCount = extractionResults.Count,
            SavedInvoices = savedInvoices,
            SkippedCount = extractionResults.Count(r => string.IsNullOrEmpty(r.Info.InvoiceNumber)),
            DuplicateInvoiceNumbers = duplicateInvoiceNumbers
        });
    }
}
