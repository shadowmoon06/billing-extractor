using BillingExtractor.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register DbContext with PostgreSQL
builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// Register Redis distributed cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "BillingExtractor:";
});

var app = builder.Build();

// Test database connections on startup
await TestConnectionsAsync(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

async Task TestConnectionsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // Test PostgreSQL connection
    await TestPostgreSqlAsync(scope.ServiceProvider, logger);

    // Test Redis connection
    await TestRedisAsync(scope.ServiceProvider, logger);
}

async Task TestPostgreSqlAsync(IServiceProvider services, ILogger logger)
{
    try
    {
        var dbContext = services.GetRequiredService<BillingDbContext>();
        var canConnect = await dbContext.Database.CanConnectAsync();

        if (canConnect)
        {
            logger.LogInformation("PostgreSQL connection successful");
        }
        else
        {
            logger.LogError("PostgreSQL connection failed: Unable to connect");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "PostgreSQL connection failed: {Message}", ex.Message);
    }
}

async Task TestRedisAsync(IServiceProvider services, ILogger logger)
{
    try
    {
        var cache = services.GetRequiredService<IDistributedCache>();

        // Test write
        var testKey = "connection_test";
        var testValue = DateTime.UtcNow.ToString("O");
        await cache.SetStringAsync(testKey, testValue, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
        });

        // Test read
        var retrieved = await cache.GetStringAsync(testKey);

        if (retrieved == testValue)
        {
            logger.LogInformation("Redis connection successful");
        }
        else
        {
            logger.LogError("Redis connection failed: Value mismatch");
        }

        // Cleanup
        await cache.RemoveAsync(testKey);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Redis connection failed: {Message}", ex.Message);
    }
}
