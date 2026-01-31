using BillingExtractor.Data.Contexts;
using BillingExtractor.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillingExtractor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController<TEntity>(SqlContext context) : ControllerBase
    where TEntity : BaseEntity
{
    protected readonly SqlContext Context = context;
    protected abstract DbSet<TEntity> DbSet { get; }

    [HttpGet]
    public virtual async Task<ActionResult<IEnumerable<TEntity>>> GetAll()
    {
        var entities = await DbSet.ToListAsync();
        return Ok(entities);
    }

    [HttpGet("{id:int}")]
    public virtual async Task<ActionResult<TEntity>> GetById(int id)
    {
        var entity = await DbSet.FindAsync(id);
        if (entity is null)
        {
            return NotFound();
        }
        return Ok(entity);
    }

    [HttpPost]
    public virtual async Task<ActionResult<TEntity>> Create(TEntity entity)
    {
        DbSet.Add(entity);
        await Context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:int}")]
    public virtual async Task<IActionResult> Update(int id, TEntity entity)
    {
        if (id != entity.Id)
        {
            return BadRequest("ID mismatch");
        }

        var existingEntity = await DbSet.FindAsync(id);
        if (existingEntity is null)
        {
            return NotFound();
        }

        entity.UpdatedAt = DateTime.Now;
        Context.Entry(existingEntity).CurrentValues.SetValues(entity);
        await Context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        var entity = await DbSet.FindAsync(id);
        if (entity is null)
        {
            return NotFound();
        }

        DbSet.Remove(entity);
        await Context.SaveChangesAsync();

        return NoContent();
    }
}
