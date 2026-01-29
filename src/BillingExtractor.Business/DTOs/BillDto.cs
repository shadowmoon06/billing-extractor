namespace BillingExtractor.Business.DTOs;

public record BillDto
{
    public int Id { get; init; }
    public string VendorName { get; init; } = string.Empty;
    public string InvoiceNumber { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime BillDate { get; init; }
    public DateTime? DueDate { get; init; }
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record CreateBillDto
{
    public required string VendorName { get; init; }
    public required string InvoiceNumber { get; init; }
    public decimal Amount { get; init; }
    public DateTime BillDate { get; init; }
    public DateTime? DueDate { get; init; }
    public string? Description { get; init; }
}

public record UpdateBillDto
{
    public string? VendorName { get; init; }
    public string? InvoiceNumber { get; init; }
    public decimal? Amount { get; init; }
    public DateTime? BillDate { get; init; }
    public DateTime? DueDate { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}
