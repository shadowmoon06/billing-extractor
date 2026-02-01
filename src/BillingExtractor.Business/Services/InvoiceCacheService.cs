using System.Text.Json;
using BillingExtractor.Business.Interfaces;
using BillingExtractor.Business.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace BillingExtractor.Business.Services;

public class InvoiceCacheService(IDistributedCache cache) : IInvoiceCacheService
{
    private const string SummaryKeyPrefix = "invoice:summary:";
    private const string DetailKeyPrefix = "invoice:detail:";
    private const string AllSummariesKey = "invoice:all_summaries";

    private static readonly TimeSpan CacheTtl = TimeSpan.FromDays(1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = CacheTtl
    };

    public async Task<InvoiceSummaryDto?> GetSummaryAsync(string invoiceNumber)
    {
        var json = await cache.GetStringAsync(SummaryKeyPrefix + invoiceNumber);
        if (string.IsNullOrEmpty(json))
            return null;

        return JsonSerializer.Deserialize<InvoiceSummaryDto>(json, JsonOptions);
    }

    public async Task<InvoiceDetailDto?> GetDetailAsync(string invoiceNumber)
    {
        var json = await cache.GetStringAsync(DetailKeyPrefix + invoiceNumber);
        if (string.IsNullOrEmpty(json))
            return null;

        return JsonSerializer.Deserialize<InvoiceDetailDto>(json, JsonOptions);
    }

    public async Task<IEnumerable<InvoiceSummaryDto>?> GetAllSummariesAsync()
    {
        var json = await cache.GetStringAsync(AllSummariesKey);
        if (string.IsNullOrEmpty(json))
            return null;

        return JsonSerializer.Deserialize<IEnumerable<InvoiceSummaryDto>>(json, JsonOptions);
    }

    public async Task SetSummaryAsync(string invoiceNumber, InvoiceSummaryDto summary)
    {
        var json = JsonSerializer.Serialize(summary, JsonOptions);
        await cache.SetStringAsync(SummaryKeyPrefix + invoiceNumber, json, CacheOptions);
    }

    public async Task SetDetailAsync(string invoiceNumber, InvoiceDetailDto detail)
    {
        var json = JsonSerializer.Serialize(detail, JsonOptions);
        await cache.SetStringAsync(DetailKeyPrefix + invoiceNumber, json, CacheOptions);
    }

    public async Task SetAllSummariesAsync(IEnumerable<InvoiceSummaryDto> summaries)
    {
        var json = JsonSerializer.Serialize(summaries, JsonOptions);
        await cache.SetStringAsync(AllSummariesKey, json, CacheOptions);
    }

    public async Task DeleteAsync(string invoiceNumber)
    {
        await Task.WhenAll(
            cache.RemoveAsync(SummaryKeyPrefix + invoiceNumber),
            cache.RemoveAsync(DetailKeyPrefix + invoiceNumber),
            cache.RemoveAsync(AllSummariesKey) // Invalidate all summaries list
        );
    }

    public async Task InvalidateAllSummariesAsync()
    {
        await cache.RemoveAsync(AllSummariesKey);
    }
}
