namespace BillingExtractor.Data.Entities;

public abstract class BaseEntity
{
    public int Id { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}
