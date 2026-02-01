using BillingExtractor.API.Configurations;
using BillingExtractor.Business;
using BillingExtractor.Data;
using BillingExtractor.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",  // Vite dev server
                "http://localhost:3000"   // Docker frontend
              )
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure ImageUploadSettings
builder.Services.Configure<ImageUploadSettings>(
    builder.Configuration.GetSection(ImageUploadSettings.SectionName));

// Register DbContext with PostgreSQL
var pgConfig = builder.Configuration.GetSection("PostgreSQL");
var pgConnectionString = $"Host={pgConfig["Host"]};Port={pgConfig["Port"]};Database={pgConfig["Database"]};Username={pgConfig["Username"]};Password={pgConfig["Password"]}";
builder.Services.AddDbContext<SqlContext>(options =>
    options.UseNpgsql(pgConnectionString));

// Register Redis distributed cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "BillingExtractor:";
});

// Register Data layer services
builder.Services.AddDataServices();

// Register Business layer services
var geminiApiKey = builder.Configuration["GeminiAPIKey"] ?? throw new InvalidOperationException("GeminiAPIKey is not configured");
builder.Services.AddBusinessServices(geminiApiKey);

var app = builder.Build();

// Apply migrations and test connections on startup
await InitializeDatabaseAsync(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS must be before other middleware
app.UseCors();

// Only use HTTPS redirection in development (Docker uses HTTP internally)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // Apply pending migrations
    await ApplyMigrationsAsync(scope.ServiceProvider, logger);

    // Test Redis connection
    await TestRedisAsync(scope.ServiceProvider, logger);
}

async Task ApplyMigrationsAsync(IServiceProvider services, ILogger logger)
{
    try
    {
        var dbContext = services.GetRequiredService<SqlContext>();

        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Migrations applied successfully");
        }
        else
        {
            logger.LogInformation("Database is up to date, no migrations to apply");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration failed: {Message}", ex.Message);
        throw; // Fail fast if migrations fail
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
