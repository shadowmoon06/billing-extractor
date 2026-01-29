namespace BillingExtractor.Data.Entities;

public class Invoice : BaseEntity
{
    public required string InvoiceNumber { get; set; }
    public required DateTime IssuedDate { get; set; }
    public required string VendorName { get; set; }
    public required decimal TotalAmount { get; set; }
    public List<InvoiceItem> Items { get; set; } = [];
}
