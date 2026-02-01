using BillingExtractor.Data.Repositories.SqlRepositories;
using BillingExtractor.Data.Repositories.SqlRepositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BillingExtractor.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();

        return services;
    }
}
