using BillingExtractor.API.Configurations;
using BillingExtractor.Data.Contexts;
using BillingExtractor.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BillingExtractor.API.Controllers;

public class InvoiceController(
    SqlContext context,
    IOptions<ImageUploadSettings> imageUploadSettings,
    IWebHostEnvironment environment) : BaseController<Invoice>(context)
{
    private readonly ImageUploadSettings _imageSettings = imageUploadSettings.Value;

    protected override DbSet<Invoice> DbSet => Context.Invoices;

    [HttpPost("upload-images")]
    public async Task<IActionResult> UploadImages(List<IFormFile> images)
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

        var uploadPath = Path.Combine(environment.ContentRootPath, _imageSettings.UploadFolder);
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        var uploadedFiles = new List<object>();
        foreach (var image in images)
        {
            var fileExtension = Path.GetExtension(image.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(stream);

            uploadedFiles.Add(new
            {
                OriginalFileName = image.FileName,
                FileName = fileName,
                FilePath = Path.Combine(_imageSettings.UploadFolder, fileName),
                ContentType = image.ContentType,
                Size = image.Length
            });
        }

        return Ok(new { UploadedFiles = uploadedFiles });
    }
}
