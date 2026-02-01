namespace BillingExtractor.API.Configurations;

public class ImageUploadSettings
{
    public const string SectionName = "ImageUpload";

    public string[] AllowedMimeTypes { get; set; } = [];
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB default
    public string UploadFolder { get; set; } = "Uploads/Invoices";
}
