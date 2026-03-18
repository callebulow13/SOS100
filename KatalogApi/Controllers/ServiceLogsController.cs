using KatalogApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KatalogApi.Models;

namespace KatalogApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServiceLogsController : ControllerBase
{
    private readonly CatalogDbContext _context;

    public ServiceLogsController(CatalogDbContext context)
    {
        _context = context;
    }

    // 1. READ: Hämta alla serviceloggar (GET: api/servicelogs)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceLogEntry>>> GetServiceLogs()
    {
        return await _context.ServiceLogs.ToListAsync();
    }

    // 2. READ: Hämta EN specifik servicelogg (GET: api/servicelogs/5)
    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceLogEntry>> GetServiceLog(int id)
    {
        var serviceLog = await _context.ServiceLogs.FindAsync(id);

        if (serviceLog == null)
        {
            return NotFound();
        }

        return serviceLog;
    }

    // SPECIAL: Hämta alla loggar för en specifik pryl (GET: api/servicelogs/item/3)
    // Den här blir guld värd för React-appen när vi klickar på en pryl!
    [HttpGet("item/{itemId}")]
    public async Task<ActionResult<IEnumerable<ServiceLogEntry>>> GetLogsForItem(int itemId)
    {
        return await _context.ServiceLogs
            .Where(log => log.ItemId == itemId)
            .ToListAsync();
    }

    // 3. CREATE: Skapa en ny servicelogg (POST: api/servicelogs)
    [HttpPost]
    public async Task<ActionResult<ServiceLogEntry>> PostServiceLog(ServiceLogEntry serviceLog)
    {
        _context.ServiceLogs.Add(serviceLog);
        await _context.SaveChangesAsync();

        // Returnerar 201 Created och visar var den nya resursen finns
        return CreatedAtAction(nameof(GetServiceLog), new { id = serviceLog.Id }, serviceLog);
    }

    // 4. UPDATE: Uppdatera en befintlig logg (PUT: api/servicelogs/5)
    [HttpPut("{id}")]
    public async Task<IActionResult> PutServiceLog(int id, ServiceLogEntry serviceLog)
    {
        if (id != serviceLog.Id)
        {
            return BadRequest("ID i URL matchar inte ID i bodyn.");
        }

        _context.Entry(serviceLog).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ServiceLogExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent(); // 204 No Content betyder att uppdateringen lyckades
    }

    // 5. DELETE: Ta bort en servicelogg (DELETE: api/servicelogs/5)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteServiceLog(int id)
    {
        var serviceLog = await _context.ServiceLogs.FindAsync(id);
        if (serviceLog == null)
        {
            return NotFound();
        }

        _context.ServiceLogs.Remove(serviceLog);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Hjälpmetod för att kolla om en logg existerar
    private bool ServiceLogExists(int id)
    {
        return _context.ServiceLogs.Any(e => e.Id == id);
    }
}