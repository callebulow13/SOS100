using KatalogApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KatalogApi.Models;

namespace KatalogApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ErrorReportsController : ControllerBase
{
    private readonly CatalogDbContext _context;

    public ErrorReportsController(CatalogDbContext context)
    {
        _context = context;
    }

    // 1. READ: Hämta alla felanmälningar (GET: api/errorreports)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ErrorReport>>> GetErrorReports()
    {
        return await _context.ErrorReports.ToListAsync();
    }

    // 2. READ: Hämta EN specifik felanmälan (GET: api/errorreports/5)
    [HttpGet("{id}")]
    public async Task<ActionResult<ErrorReport>> GetErrorReport(int id)
    {
        var errorReport = await _context.ErrorReports.FindAsync(id);

        if (errorReport == null)
        {
            return NotFound();
        }

        return errorReport;
    }

    // SPECIAL: Hämta alla felanmälningar för en specifik pryl (GET: api/errorreports/item/3)
    [HttpGet("item/{itemId}")]
    public async Task<ActionResult<IEnumerable<ErrorReport>>> GetReportsForItem(int itemId)
    {
        return await _context.ErrorReports
            .Where(report => report.ItemId == itemId)
            .ToListAsync();
    }

    // 3. CREATE: Skapa en ny felanmälan (POST: api/errorreports)
    // Denna kommer din MVC-klient (och React-appen om vi vill) att använda!
    [HttpPost]
    public async Task<ActionResult<ErrorReport>> PostErrorReport(ErrorReport errorReport)
    {
        _context.ErrorReports.Add(errorReport);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetErrorReport), new { id = errorReport.Id }, errorReport);
    }

    // 4. UPDATE: Uppdatera en felanmälan (PUT: api/errorreports/5)
    // Perfekt för IT-teamet i React när de klickar "Markera som löst" (IsResolved = true)
    [HttpPut("{id}")]
    public async Task<IActionResult> PutErrorReport(int id, ErrorReport errorReport)
    {
        if (id != errorReport.Id)
        {
            return BadRequest("ID i URL matchar inte ID i bodyn.");
        }

        _context.Entry(errorReport).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ErrorReportExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // 5. DELETE: Ta bort en felanmälan (DELETE: api/errorreports/5)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteErrorReport(int id)
    {
        var errorReport = await _context.ErrorReports.FindAsync(id);
        if (errorReport == null)
        {
            return NotFound();
        }

        _context.ErrorReports.Remove(errorReport);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Hjälpmetod
    private bool ErrorReportExists(int id)
    {
        return _context.ErrorReports.Any(e => e.Id == id);
    }
}