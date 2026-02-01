using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BillingExtractor.Data.Contexts;

public class SqlContextFactory : IDesignTimeDbContextFactory<SqlContext>
{
    public SqlContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SqlContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=billingextractor;Username=postgres;Password=abcd0000");

        return new SqlContext(optionsBuilder.Options);
    }
}
