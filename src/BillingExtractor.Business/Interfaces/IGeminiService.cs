namespace BillingExtractor.Business.Interfaces;

public interface IGeminiService
{
    Task<string> GenerateContentAsync(string prompt);
    Task<string> GenerateContentFromImageAsync(string prompt, byte[] imageBytes, string mimeType);
}
