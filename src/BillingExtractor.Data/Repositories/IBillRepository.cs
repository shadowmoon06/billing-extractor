using BillingExtractor.Data.Entities;

namespace BillingExtractor.Data.Repositories;

public interface IBillRepository
{
    Task<IEnumerable<Bill>> GetAllAsync();
    Task<Bill?> GetByIdAsync(int id);
    Task<Bill> AddAsync(Bill bill);
    Task<Bill> UpdateAsync(Bill bill);
    Task DeleteAsync(int id);
    Task<IEnumerable<Bill>> GetByStatusAsync(BillStatus status);
}
