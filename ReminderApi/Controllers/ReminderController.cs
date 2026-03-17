using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReminderApi.Data;
using ReminderApi.Models;
using ReminderApi.Filters;

namespace ReminderApi.Controllers;
// 
[ApiController]
[Route("api/[controller]")]
[ServiceFilter(typeof(ApiKeyFilter))]
public class RemindersController : ControllerBase
{
    private readonly AppDbContext _db;

    public RemindersController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/reminders
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? userId,
        [FromQuery] bool? overdue)
    {
        var query = _db.Reminders.Include(r => r.Watch).AsQueryable();

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(r => r.UserId == userId);

        if (overdue == true)
            query = query.Where(r => r.DueDate < DateTime.UtcNow && !r.IsSent);

        return Ok(await query.OrderBy(r => r.DueDate).ToListAsync());
    }

    // GET /api/reminders/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOne(int id)
    {
        var reminder = await _db.Reminders
            .Include(r => r.Watch)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reminder == null)
            return NotFound(new { error = "Hittades inte." });

        return Ok(reminder);
    }

    // POST /api/reminders
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Reminder reminder)
    {
        _db.Reminders.Add(reminder);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOne), new { id = reminder.Id }, reminder);
    }
    

    // PUT /api/reminders/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Reminder updated)
    {
        var reminder = await _db.Reminders.FindAsync(id);
        if (reminder == null) return NotFound();

        reminder.IsSent = updated.IsSent;
        reminder.DueDate = updated.DueDate;
        reminder.ItemTitle = updated.ItemTitle;

        await _db.SaveChangesAsync();
        return Ok(reminder);
    }

    // DELETE /api/reminders/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var reminder = await _db.Reminders.FindAsync(id);
        if (reminder == null) return NotFound();

        _db.Reminders.Remove(reminder);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // GET /api/reminders/overdue/count
    [HttpGet("overdue/count")]
    public async Task<IActionResult> OverdueCount()
    {
        var count = await _db.Reminders
            .CountAsync(r => r.DueDate < DateTime.UtcNow && !r.IsSent);
        return Ok(new { count });
    }
}