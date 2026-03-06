using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReminderApi.Data;
using ReminderApi.Models;
using ReminderApi.Filters;

namespace ReminderApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[ServiceFilter(typeof(ApiKeyFilter))]
public class WatchesController : ControllerBase
{
    private readonly AppDbContext _db;

    public WatchesController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/watches
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? userId)
    {
        var query = _db.Watches
            .Include(w => w.Reminders)
            .AsQueryable();

        if (userId.HasValue)
            query = query.Where(w => w.UserId == userId.Value);

        return Ok(await query.ToListAsync());
    }

    // GET /api/watches/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOne(int id)
    {
        var watch = await _db.Watches
            .Include(w => w.Reminders)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (watch == null)
            return NotFound(new { error = "Bevakning hittades inte." });

        return Ok(watch);
    }

    // POST /api/watches
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Watch watch)
    {
        _db.Watches.Add(watch);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOne), new { id = watch.Id }, watch);
    }

    // PUT /api/watches/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Watch updated)
    {
        var watch = await _db.Watches.FindAsync(id);
        if (watch == null)
            return NotFound(new { error = "Bevakning hittades inte." });

        watch.IsActive = updated.IsActive;
        watch.ItemTitle = updated.ItemTitle;

        await _db.SaveChangesAsync();
        return Ok(watch);
    }

    // DELETE /api/watches/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var watch = await _db.Watches
            .Include(w => w.Reminders)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (watch == null)
            return NotFound(new { error = "Bevakning hittades inte." });

        // Ta bort kopplade påminnelser först
        _db.Reminders.RemoveRange(watch.Reminders);
        _db.Watches.Remove(watch);
    
        await _db.SaveChangesAsync();
        return NoContent();
    }
}