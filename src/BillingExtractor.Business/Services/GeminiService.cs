using BillingExtractor.Business.Interfaces;
using Google.GenAI;
using Google.GenAI.Types;

namespace BillingExtractor.Business.Services;

public class GeminiService(string apiKey) : IGeminiService
{
    private readonly Client _client = new(apiKey: apiKey);
    private const string Model = "gemini-2.0-flash";

    public async Task<string> GenerateContentAsync(string prompt)
    {
        var response = await _client.Models.GenerateContentAsync(model: Model, contents: prompt);
        return response.Candidates?[0].Content?.Parts?[0].Text ?? string.Empty;
    }

    public async Task<string> GenerateContentFromImageAsync(string prompt, byte[] imageBytes, string mimeType)
    {
        var contents = new Content
        {
            Parts =
            [
                new Part { InlineData = new Blob { Data = imageBytes, MimeType = mimeType } },
                new Part { Text = prompt }
            ]
        };

        var response = await _client.Models.GenerateContentAsync(model: Model, contents: contents);
        return response.Candidates?[0].Content?.Parts?[0].Text ?? string.Empty;
    }
}
