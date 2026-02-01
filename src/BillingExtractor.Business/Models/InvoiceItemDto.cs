namespace BillingExtractor.Business.Models;

public class InvoiceItemDto
{
    public required string ItemId { get; set; }
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Unit { get; set; }
    public decimal Amount { get; set; }
}
