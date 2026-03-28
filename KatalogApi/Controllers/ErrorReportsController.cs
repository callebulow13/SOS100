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

    // Hämtar en lista med alla felrapporter i hela systemet.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ErrorReport>>> GetErrorReports()
    {
        return await _context.ErrorReports.ToListAsync();
    }

    // Hämtar en specifik felrapport via dess unika ID.
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

    // Hämta felrapporter som tillhör ett specifikt objekt.
    [HttpGet("item/{itemId}")]
    public async Task<ActionResult<IEnumerable<ErrorReport>>> GetReportsForItem(int itemId)
    {
        return await _context.ErrorReports
            .Where(report => report.ItemId == itemId)
            .ToListAsync();
    }

    // Skapar och sparar en ny felrapport.
    [HttpPost]
    public async Task<ActionResult<ErrorReport>> PostErrorReport(ErrorReport errorReport)
    {
        _context.ErrorReports.Add(errorReport);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetErrorReport), new { id = errorReport.Id }, errorReport);
    }

    // Uppdaterar en befintlig felrapport.
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

    // Tar bort en felrapport från systemet.
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

    private bool ErrorReportExists(int id)
    {
        return _context.ErrorReports.Any(e => e.Id == id);
    }
}