using BillingExtractor.Business.DTOs;

namespace BillingExtractor.Business.Services;

public interface IBillService
{
    Task<IEnumerable<BillDto>> GetAllBillsAsync();
    Task<BillDto?> GetBillByIdAsync(int id);
    Task<BillDto> CreateBillAsync(CreateBillDto createBillDto);
    Task<BillDto?> UpdateBillAsync(int id, UpdateBillDto updateBillDto);
    Task<bool> DeleteBillAsync(int id);
    Task<IEnumerable<BillDto>> GetBillsByStatusAsync(string status);
}
