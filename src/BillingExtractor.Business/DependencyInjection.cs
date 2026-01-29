using BillingExtractor.Business.Interfaces;
using BillingExtractor.Business.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BillingExtractor.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services, string geminiApiKey)
    {
        services.AddSingleton<IGeminiService>(new GeminiService(geminiApiKey));

        return services;
    }
}
