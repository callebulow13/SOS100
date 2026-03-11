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
    // Uppdatera en befintlig pryl (PUT)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] Item updatedItem)
    {
        // 1. Kontrollera att ID:t i webbadressen stämmer överens med ID:t i objektet vi skickar in
        if (id != updatedItem.Id)
        {
            return BadRequest("ID i webbadressen matchar inte objektets ID.");
        }

        // 2. Leta upp den befintliga prylen i databasen
        var item = await _context.Items.FindAsync(id);
        
        if (item == null)
        {
            return NotFound($"Kunde inte hitta någon pryl med ID {id} att uppdatera.");
        }

        // 3. Uppdatera alla egenskaper på prylen
        item.Name = updatedItem.Name;
        item.Type = updatedItem.Type;
        item.Description = updatedItem.Description;
        item.Status = updatedItem.Status;
        item.Placement = updatedItem.Placement;
        item.PurchaseDate = updatedItem.PurchaseDate;

        // 4. Spara ändringarna i SQLite-filen
        await _context.SaveChangesAsync();

        // 5. Returnera 204 No Content (Standard för lyckade PUT-anrop där vi inte behöver skicka tillbaka data)
        return NoContent();
    }
    // Ta bort en pryl (DELETE)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        // 1. Leta upp prylen i databasen
        var item = await _context.Items.FindAsync(id);
        
        if (item == null)
        {
            return NotFound($"Kunde inte hitta någon pryl med ID {id} att ta bort.");
        }

        // 2. Säg till databas-bron att vi vill radera prylen
        _context.Items.Remove(item);
        
        // 3. Spara ändringarna till SQLite-filen
        await _context.SaveChangesAsync();

        // 4. Returnera 204 No Content
        return NoContent();
    }
    
    // POST: api/items/seed
    [HttpPost("seed")]
    public async Task<IActionResult> SeedDatabase()
    {
        // 1. Kontrollera om det redan finns data, avbryt i så fall för att undvika dubbletter
        if (await _context.Items.AnyAsync())
        {
            return BadRequest("Databasen har redan data. Rensa den först!");
        }

        // 2. Skapa en lista med test-prylar som matchar exakt de enums och properties ni har
        var dummyItems = new List<Item>
        {
            new Item 
            { 
                Name = "C# för nybörjare", 
                Type = ItemType.Bok, 
                Status = ItemStatus.Tillgänglig,
                Description = "Grundläggande bok om C# och .NET",
                Placement = "Bokhylla A1",
                PurchaseDate = DateTime.Now.AddDays(-120)
            },
            new Item 
            { 
                Name = "Bärbar Projektor", 
                Type = ItemType.Elektronik, 
                Status = ItemStatus.Tillgänglig,
                Description = "Epson 1080p för presentationer utanför huset",
                Placement = "IT-skåpet",
                PurchaseDate = DateTime.Now.AddDays(-300)
            },
            new Item 
            { 
                Name = "Kvartalsrapport Q1", 
                Type = ItemType.Rapport, 
                Status = ItemStatus.Tillgänglig,
                Description = "Ekonomisk rapport för första kvartalet",
                Placement = "Arkiv 2",
                PurchaseDate = DateTime.Now.AddDays(-15)
            },
            new Item 
            { 
                Name = "Whiteboard-pennor (10-pack)", 
                Type = ItemType.Annat, 
                Status = ItemStatus.Tillgänglig,
                Description = "Flerfärgade pennor för konferensrummet",
                Placement = "Förråd B",
                PurchaseDate = DateTime.Now.AddDays(-5)
            },
            new Item 
            { 
                Name = "Systemkamera Sony", 
                Type = ItemType.Elektronik, 
                Status = ItemStatus.Trasig, // Vi lägger in en trasig för att se hur din MVC-design hanterar det!
                Description = "Används av marknadsavdelningen. Objektivet är skadat.",
                Placement = "IT-supporten",
                PurchaseDate = DateTime.Now.AddDays(-600)
            },
            new Item 
            { 
                Name = "Surfplatta iPad Pro", 
                Type = ItemType.Elektronik, 
                Status = ItemStatus.Tillgänglig,
                Description = "iPad Pro 12.9 tum med Apple Pencil, perfekt för skisser",
                Placement = "IT-skåpet",
                PurchaseDate = DateTime.Now.AddDays(-45)
            },
            new Item 
            { 
                Name = "Clean Code (Bok)", 
                Type = ItemType.Bok, 
                Status = ItemStatus.Utlånad, // Testar filter för utlånade objekt
                Description = "Klassisk bok om mjukvaruarkitektur av Robert C. Martin",
                Placement = "Bokhylla C3",
                PurchaseDate = DateTime.Now.AddDays(-800)
            },
            new Item 
            { 
                Name = "Årsredovisning 2025", 
                Type = ItemType.Rapport, 
                Status = ItemStatus.Tillgänglig,
                Description = "Fysisk kopia av förra årets ekonomiska sammanställning",
                Placement = "Arkiv 1",
                PurchaseDate = DateTime.Now.AddDays(-60)
            },
            new Item 
            { 
                Name = "Ergonomisk kontorsstol", 
                Type = ItemType.Annat, 
                Status = ItemStatus.Tillgänglig,
                Description = "Extra stol för gästarbetsplatser (Herman Miller)",
                Placement = "Konferensrum Oden",
                PurchaseDate = DateTime.Now.AddDays(-150)
            },
            new Item 
            { 
                Name = "Konferensmikrofon Jabra", 
                Type = ItemType.Elektronik, 
                Status = ItemStatus.Saknas, // Testar röd status för saknade objekt
                Description = "Trådlös puck-mikrofon för hybridmöten",
                Placement = "Okänd",
                PurchaseDate = DateTime.Now.AddDays(-300)
            },
            new Item 
            { 
                Name = "Design Patterns (GoF)", 
                Type = ItemType.Bok, 
                Status = ItemStatus.Tillgänglig,
                Description = "Elements of Reusable Object-Oriented Software",
                Placement = "Bokhylla A2",
                PurchaseDate = DateTime.Now.AddDays(-1200)
            },
            new Item 
            { 
                Name = "Första hjälpen-väska", 
                Type = ItemType.Annat, 
                Status = ItemStatus.Tillgänglig,
                Description = "Mobil sjukvårdsväska för event och utflykter",
                Placement = "Receptionen",
                PurchaseDate = DateTime.Now.AddDays(-20)
            },
            new Item 
            { 
                Name = "Bärbar extraskärm 15\"", 
                Type = ItemType.Elektronik, 
                Status = ItemStatus.Tillgänglig,
                Description = "ASUS ZenScreen, ansluts via USB-C",
                Placement = "IT-skåpet",
                PurchaseDate = DateTime.Now.AddDays(-90)
            },
            new Item 
            { 
                Name = "Säkerhetsrevision Q4", 
                Type = ItemType.Rapport, 
                Status = ItemStatus.Tillgänglig,
                Description = "Sammanställning av penetrationstester och IT-säkerhet",
                Placement = "Arkiv 3 (Låst)",
                PurchaseDate = DateTime.Now.AddDays(-10)
            },
            new Item 
            { 
                Name = "Pro ASP.NET Core 8", 
                Type = ItemType.Bok, 
                Status = ItemStatus.Tillgänglig,
                Description = "Djupdykning i MVC och webbutveckling med C#",
                Placement = "Bokhylla A1",
                PurchaseDate = DateTime.Now.AddDays(-5)
            }
        };

        // 3. Lägg till alla prylar i databas-bron och spara
        _context.Items.AddRange(dummyItems);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Lade till {dummyItems.Count} test-prylar i katalogen!" });
    }

    // DELETE: api/items/clear
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearDatabase()
    {
        // 1. Hämta alla existerande rader i tabellen
        var allItems = await _context.Items.ToListAsync();
        
        // 2. Säg till Entity Framework att ta bort hela listan
        _context.Items.RemoveRange(allItems);
        
        // 3. Spara ändringarna i SQLite-filen
        await _context.SaveChangesAsync();

        return Ok(new { message = "Katalogen är nu helt rensad!" });
    }
}