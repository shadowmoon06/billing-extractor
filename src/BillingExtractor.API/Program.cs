using BillingExtractor.Business.Services;
using BillingExtractor.Data;
using BillingExtractor.Data.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register DbContext with In-Memory database
builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseInMemoryDatabase("BillingExtractorDb"));

// Register Data layer services
builder.Services.AddScoped<IBillRepository, BillRepository>();

// Register Business layer services
builder.Services.AddScoped<IBillService, BillService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
