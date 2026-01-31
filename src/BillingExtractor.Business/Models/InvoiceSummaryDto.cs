namespace BillingExtractor.Business.Models;

public class InvoiceSummaryDto
{
    public required string InvoiceNumber { get; set; }
    public required DateTime IssuedDate { get; set; }
    public required string VendorName { get; set; }
    public required decimal TotalAmount { get; set; }
    public required DateTime LastEdited { get; set; }
}
