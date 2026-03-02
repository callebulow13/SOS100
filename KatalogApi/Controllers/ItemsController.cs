using KatalogApi.Data;
using KatalogApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KatalogApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ItemsController : ControllerBase
{
    private readonly CatalogDbContext _context;

    // Konstruktorn ber om databas-bron när API:et startar
    public ItemsController(CatalogDbContext context)
    {
        _context = context;
    }

    // Hämta objekt
    [HttpGet]
    public async Task<IActionResult> GetItems()
    {
        // Hämta alla prylar direkt från databasen!
        var items = await _context.Items.ToListAsync();
        
        return Ok(items);
    }
    // Lägga till objekt
    [HttpPost]
    public async Task<IActionResult> CreateItem([FromBody] Item newItem)
    {
        // Sätt Id till 0. Databasen kommer automatiskt att räkna ut och ge prylen nästa lediga nummer (t.ex. 4).
        newItem.Id = 0; 

        // Säg till databas-bron att vi vill lägga till en ny pryl
        _context.Items.Add(newItem);

        // Spara ändringarna i SQLite-filen!
        await _context.SaveChangesAsync();

        // Returnera statuskod "201 Created" och skicka tillbaka prylen 
        // (nu med sitt nya, riktiga databas-ID) till den som skapade den.
        return Created($"/api/items/{newItem.Id}", newItem);
    }
}