using BillingExtractor.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BillingExtractor.Data.Contexts;

public class SqlContext(DbContextOptions<SqlContext> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.VendorName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);

            entity.HasIndex(e => e.InvoiceNumber);
            entity.HasIndex(e => e.VendorName);

            entity.OwnsMany(e => e.Items, items =>
            {
                items.ToJson();
            });
        });
    }
}
