namespace BillingExtractor.Business.Models;

public class InvoiceExtractedInfo
{
    public string? InvoiceNumber { get; set; }
    public DateTime? IssuedDate { get; set; }
    public string? VendorName { get; set; }
    public decimal? TotalAmount { get; set; }
    public List<InvoiceItemInfo> Items { get; set; } = [];
}

public class InvoiceItemInfo
{
    public string? ItemId { get; set; }
    public string? Description { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Unit { get; set; }
    public decimal? Amount { get; set; }
}
