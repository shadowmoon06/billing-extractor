using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingExtractor.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceAdjustments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Adjustments",
                table: "Invoices",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Adjustments",
                table: "Invoices");
        }
    }
}
