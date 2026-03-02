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
    
    // Denna [HttpGet] lyssnar efter ett specifikt ID i webbadressen, t.ex. /api/items/3
    [HttpGet("{id}")]
    public async Task<IActionResult> GetItemById(int id)
    {
        // 1. Vi ber databasen leta upp prylen som har exakt detta ID.
        // FindAsync är supersnabb eftersom den letar direkt efter primärnyckeln.
        var item = await _context.Items.FindAsync(id);

        // 2. Om databasen returnerar null, betyder det att ID:t inte finns.
        if (item == null)
        {
            // Vi skickar tillbaka HTTP 404 (Not Found) och ett litet meddelande
            return NotFound($"Kunde inte hitta någon pryl med ID {id} i katalogen.");
        }

        // 3. Om prylen finns, skickar vi tillbaka den!
        // Ok() ger HTTP 200 och förvandlar automatiskt C#-objektet till JSON-kod.
        return Ok(item);
    }
}