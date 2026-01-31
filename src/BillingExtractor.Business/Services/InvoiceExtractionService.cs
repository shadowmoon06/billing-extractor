using System.Text.Json;
using BillingExtractor.Business.Interfaces;
using BillingExtractor.Business.Models;

namespace BillingExtractor.Business.Services;

public class InvoiceExtractionService(IGeminiService geminiService) : IInvoiceExtractionService
{
    private const string ExtractionPrompt = """
        Analyze this invoice image and extract the following information in JSON format:
        {
            "invoiceNumber": "string or null",
            "issuedDate": "YYYY-MM-DD format or null",
            "vendorName": "string or null",
            "totalAmount": number or null,
            "items": [
                {
                    "itemId": "string or null",
                    "description": "string or null",
                    "quantity": number or null,
                    "unitPrice": number or null,
                    "unit": "string or null (e.g., lbs, gallons, pcs)",
                    "amount": number or null
                }
            ]
        }

        Return ONLY valid JSON without any markdown formatting or code blocks.
        If a field cannot be determined from the image, use null.
        """;

    public async Task<InvoiceExtractedInfo> ExtractFromImageAsync(byte[] imageBytes, string mimeType)
    {
        var response = await geminiService.GenerateContentFromImageAsync(ExtractionPrompt, imageBytes, mimeType);
        return ParseResponse(response);
    }

    public async Task<InvoiceExtractedInfo> ExtractFromFilePathAsync(string filePath)
    {
        var imageBytes = await File.ReadAllBytesAsync(filePath);
        var mimeType = GetMimeType(filePath);
        return await ExtractFromImageAsync(imageBytes, mimeType);
    }

    private static InvoiceExtractedInfo ParseResponse(string response)
    {
        try
        {
            var cleanedResponse = response.Trim();
            if (cleanedResponse.StartsWith("```"))
            {
                var lines = cleanedResponse.Split('\n');
                cleanedResponse = string.Join('\n', lines.Skip(1).Take(lines.Length - 2));
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<InvoiceExtractedInfo>(cleanedResponse, options)
                   ?? new InvoiceExtractedInfo();
        }
        catch (JsonException)
        {
            return new InvoiceExtractedInfo();
        }
    }

    private static string GetMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };
    }
}
