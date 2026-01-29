namespace BillingExtractor.Data.Entities;

public class InvoiceItem
{
    public string ItemId { get; set; }
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Unit { get; set; } // lbs, gallons
    public decimal Amount { get; set; }
}
