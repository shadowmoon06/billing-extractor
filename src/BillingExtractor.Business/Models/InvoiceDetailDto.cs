namespace BillingExtractor.Business.Models;

public class InvoiceDetailDto
{
    public required string InvoiceNumber { get; set; }
    public required DateTime IssuedDate { get; set; }
    public required string VendorName { get; set; }
    public required decimal TotalAmount { get; set; }
    public required DateTime LastEdited { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = [];
    public List<InvoiceAdjustmentDto> Adjustments { get; set; } = [];
}
