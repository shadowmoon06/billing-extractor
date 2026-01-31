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

        var results = new List<object>();
        foreach (var image in images)
        {
            using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            var extractedInfo = await invoiceExtractionService.ExtractFromImageAsync(imageBytes, image.ContentType);

            results.Add(new
            {
                FileName = image.FileName,
                ContentType = image.ContentType,
                Size = image.Length,
                ExtractedInfo = extractedInfo
            });
        }

        return Ok(new { Results = results });
    }
}
