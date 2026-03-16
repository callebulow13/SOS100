using KatalogApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KatalogApi.Models;
using KatalogApi.Data;

namespace KatalogApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CoordinatesController : ControllerBase
{
    // Byt ut "KatalogDbContext" till vad din DbContext-klass faktiskt heter!
    private readonly CatalogDbContext _context;

    public CoordinatesController(CatalogDbContext context)
    {
        _context = context;
    }

    // GET: api/coordinates
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LocationCoordinate>>> GetCoordinates()
    {
        return await _context.LocationCoordinates.ToListAsync();
    }
}