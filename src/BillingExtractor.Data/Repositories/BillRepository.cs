using BillingExtractor.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BillingExtractor.Data.Repositories;

public class BillRepository : IBillRepository
{
    private readonly BillingDbContext _context;

    public BillRepository(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Bill>> GetAllAsync()
    {
        return await _context.Bills.ToListAsync();
    }

    public async Task<Bill?> GetByIdAsync(int id)
    {
        return await _context.Bills.FindAsync(id);
    }

    public async Task<Bill> AddAsync(Bill bill)
    {
        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();
        return bill;
    }

    public async Task<Bill> UpdateAsync(Bill bill)
    {
        bill.UpdatedAt = DateTime.UtcNow;
        _context.Bills.Update(bill);
        await _context.SaveChangesAsync();
        return bill;
    }

    public async Task DeleteAsync(int id)
    {
        var bill = await _context.Bills.FindAsync(id);
        if (bill != null)
        {
            _context.Bills.Remove(bill);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Bill>> GetByStatusAsync(BillStatus status)
    {
        return await _context.Bills.Where(b => b.Status == status).ToListAsync();
    }
}
