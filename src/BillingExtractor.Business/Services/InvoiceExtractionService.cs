using System.Text.Json;
using BillingExtractor.Business.Interfaces;
using BillingExtractor.Business.Models;

namespace BillingExtractor.Business.Services;

public class InvoiceExtractionService(IGeminiService geminiService) : IInvoiceExtractionService
{
    private const string PromptFileName = "Prompts/InvoiceExtraction.txt";
    private static string? _cachedPrompt;

    public async Task<InvoiceExtractedInfo> ExtractFromImageAsync(byte[] imageBytes, string mimeType)
    {
        var prompt = await GetPromptAsync();
        var response = await geminiService.GenerateContentFromImageAsync(prompt, imageBytes, mimeType);
        return ParseResponse(response);
    }

    private static async Task<string> GetPromptAsync()
    {
        if (_cachedPrompt is not null)
            return _cachedPrompt;

        var promptPath = Path.Combine(AppContext.BaseDirectory, PromptFileName);
        _cachedPrompt = await File.ReadAllTextAsync(promptPath);
        return _cachedPrompt;
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
