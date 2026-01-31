using BillingExtractor.API.Configurations;
using BillingExtractor.Business.Interfaces;
using BillingExtractor.Data.Contexts;
using BillingExtractor.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BillingExtractor.API.Controllers;

public class InvoiceController(
    SqlContext context,
    IOptions<ImageUploadSettings> imageUploadSettings,
    IInvoiceExtractionService invoiceExtractionService) : BaseController<Invoice>(context)
{
    private readonly ImageUploadSettings _imageSettings = imageUploadSettings.Value;

    protected override DbSet<Invoice> DbSet => Context.Invoices;

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
