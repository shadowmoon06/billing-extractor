namespace BillingExtractor.Data.Entities;

public class Bill
{
    public int Id { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime BillDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Description { get; set; }
    public BillStatus Status { get; set; } = BillStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum BillStatus
{
    Pending,
    Processed,
    Paid,
    Overdue,
    Cancelled
}
