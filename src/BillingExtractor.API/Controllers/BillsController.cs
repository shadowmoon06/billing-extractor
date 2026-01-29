using BillingExtractor.Business.DTOs;
using BillingExtractor.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace BillingExtractor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BillsController : ControllerBase
{
    private readonly IBillService _billService;

    public BillsController(IBillService billService)
    {
        _billService = billService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BillDto>>> GetAll()
    {
        var bills = await _billService.GetAllBillsAsync();
        return Ok(bills);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BillDto>> GetById(int id)
    {
        var bill = await _billService.GetBillByIdAsync(id);
        if (bill is null)
            return NotFound();

        return Ok(bill);
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<BillDto>>> GetByStatus(string status)
    {
        var bills = await _billService.GetBillsByStatusAsync(status);
        return Ok(bills);
    }

    [HttpPost]
    public async Task<ActionResult<BillDto>> Create([FromBody] CreateBillDto createBillDto)
    {
        var bill = await _billService.CreateBillAsync(createBillDto);
        return CreatedAtAction(nameof(GetById), new { id = bill.Id }, bill);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BillDto>> Update(int id, [FromBody] UpdateBillDto updateBillDto)
    {
        var bill = await _billService.UpdateBillAsync(id, updateBillDto);
        if (bill is null)
            return NotFound();

        return Ok(bill);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _billService.DeleteBillAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
