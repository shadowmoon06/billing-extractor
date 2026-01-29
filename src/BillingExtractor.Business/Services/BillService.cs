using BillingExtractor.Business.DTOs;
using BillingExtractor.Data.Entities;
using BillingExtractor.Data.Repositories;

namespace BillingExtractor.Business.Services;

public class BillService : IBillService
{
    private readonly IBillRepository _billRepository;

    public BillService(IBillRepository billRepository)
    {
        _billRepository = billRepository;
    }

    public async Task<IEnumerable<BillDto>> GetAllBillsAsync()
    {
        var bills = await _billRepository.GetAllAsync();
        return bills.Select(MapToDto);
    }

    public async Task<BillDto?> GetBillByIdAsync(int id)
    {
        var bill = await _billRepository.GetByIdAsync(id);
        return bill is null ? null : MapToDto(bill);
    }

    public async Task<BillDto> CreateBillAsync(CreateBillDto createBillDto)
    {
        var bill = new Bill
        {
            VendorName = createBillDto.VendorName,
            InvoiceNumber = createBillDto.InvoiceNumber,
            Amount = createBillDto.Amount,
            BillDate = createBillDto.BillDate,
            DueDate = createBillDto.DueDate,
            Description = createBillDto.Description,
            Status = BillStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var createdBill = await _billRepository.AddAsync(bill);
        return MapToDto(createdBill);
    }

    public async Task<BillDto?> UpdateBillAsync(int id, UpdateBillDto updateBillDto)
    {
        var existingBill = await _billRepository.GetByIdAsync(id);
        if (existingBill is null)
            return null;

        if (updateBillDto.VendorName is not null)
            existingBill.VendorName = updateBillDto.VendorName;
        if (updateBillDto.InvoiceNumber is not null)
            existingBill.InvoiceNumber = updateBillDto.InvoiceNumber;
        if (updateBillDto.Amount.HasValue)
            existingBill.Amount = updateBillDto.Amount.Value;
        if (updateBillDto.BillDate.HasValue)
            existingBill.BillDate = updateBillDto.BillDate.Value;
        if (updateBillDto.DueDate.HasValue)
            existingBill.DueDate = updateBillDto.DueDate;
        if (updateBillDto.Description is not null)
            existingBill.Description = updateBillDto.Description;
        if (updateBillDto.Status is not null && Enum.TryParse<BillStatus>(updateBillDto.Status, true, out var status))
            existingBill.Status = status;

        var updatedBill = await _billRepository.UpdateAsync(existingBill);
        return MapToDto(updatedBill);
    }

    public async Task<bool> DeleteBillAsync(int id)
    {
        var existingBill = await _billRepository.GetByIdAsync(id);
        if (existingBill is null)
            return false;

        await _billRepository.DeleteAsync(id);
        return true;
    }

    public async Task<IEnumerable<BillDto>> GetBillsByStatusAsync(string status)
    {
        if (!Enum.TryParse<BillStatus>(status, true, out var billStatus))
            return [];

        var bills = await _billRepository.GetByStatusAsync(billStatus);
        return bills.Select(MapToDto);
    }

    private static BillDto MapToDto(Bill bill)
    {
        return new BillDto
        {
            Id = bill.Id,
            VendorName = bill.VendorName,
            InvoiceNumber = bill.InvoiceNumber,
            Amount = bill.Amount,
            BillDate = bill.BillDate,
            DueDate = bill.DueDate,
            Description = bill.Description,
            Status = bill.Status.ToString(),
            CreatedAt = bill.CreatedAt,
            UpdatedAt = bill.UpdatedAt
        };
    }
}
