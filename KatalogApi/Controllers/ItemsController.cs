using KatalogApi.Data;
using KatalogApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KatalogApi.Controllers;

// [Route] bestämmer bas-URL:en för alla anrop i denna klass.
// "[controller]" byts automatiskt ut mot namnet på klassen minus "Controller", alltså "api/Items".
[Route("api/[controller]")]
[ApiController]
public class ItemsController : ControllerBase
{
    private readonly CatalogDbContext _context;

    public ItemsController(CatalogDbContext context)
    {
        _context = context;
    }
    
    // Hämtar alla objekt från bibliotekets katalog.
    [HttpGet]
    public async Task<IActionResult> GetItems()
    {
        // Hämtar allt från tabellen Items och gör om det till en lista.
        var items = await _context.Items.ToListAsync();

        return Ok(items);
    }
    // Lägger till nytt objekt i katalogen.
    [HttpPost]
    public async Task<IActionResult> CreateItem([FromBody] Item newItem)
    {
        // Tvingar ID till 0 för att säkerställa att databasen genererar ett nytt, unikt ID.
        newItem.Id = 0;

        _context.Items.Add(newItem);

        await _context.SaveChangesAsync();

        return Created($"/api/items/{newItem.Id}", newItem);
    }
    
    // Hämtar ett specifikt objekt baserat på dess ID.
    [HttpGet("{id}")]
    public async Task<IActionResult> GetItemById(int id)
    {
        // Letar efter objektet med den angivna primärnyckeln.
        var item = await _context.Items.FindAsync(id);

        if (item == null)
        {
            return NotFound($"Kunde inte hitta någon pryl med ID {id} i katalogen.");
        }

        return Ok(item);
    }
    // Uppdaterar informationen om ett befintligt objekt.
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] Item updatedItem)
    {
        // Säkerhetskontroll: Matcha ID:t i URL:en med ID:t i det skickade objektet.
        if (id != updatedItem.Id)
        {
            return BadRequest("ID i webbadressen matchar inte objektets ID.");
        }

        // Letar upp det befintliga objektet i databasen.
        var item = await _context.Items.FindAsync(id);

        if (item == null)
        {
            return NotFound($"Kunde inte hitta någon pryl med ID {id} att uppdatera.");
        }
        
        // Skriver över de gamla värdena med de nya.
        item.Name = updatedItem.Name;
        item.Type = updatedItem.Type;
        item.Description = updatedItem.Description;
        item.Status = updatedItem.Status;
        item.Placement = updatedItem.Placement;
        item.PurchaseDate = updatedItem.PurchaseDate;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Tar bort en pryl från katalogen.
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var item = await _context.Items.FindAsync(id);

        if (item == null)
        {
            return NotFound($"Kunde inte hitta någon pryl med ID {id} att ta bort.");
        }

        _context.Items.Remove(item);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Fyller databasen med testdata (Seeding).
    [HttpPost("seed")]
    public async Task<IActionResult> SeedDatabase()
    {
        if (await _context.Items.AnyAsync())
        {
            return BadRequest("Databasen har redan data. Rensa den först!");
        }

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
                Status = ItemStatus.Trasig,
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
                Status = ItemStatus.Utlånad,
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
                Status = ItemStatus.Tillgänglig,
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
                Status = ItemStatus.Saknas,
                Description = "Mobil sjukvårdsväska för event och utflykter",
                Placement = "Receptionen",
                PurchaseDate = DateTime.Now.AddDays(-20)
            },
            new Item
            {
                Name = "Bärbar extraskärm 15",
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
                Name = "Trådlöst Myggsystem",
                Type = ItemType.Elektronik,
                Status = ItemStatus.Tillgänglig,
                Description = "Sennheiser trådlöst mikrofonsystem för föreläsningar och digitala möten",
                Placement = "Mediarummet Hylla B",
                PurchaseDate = DateTime.Now.AddDays(-120)
            },
            new Item
            {
                Name = "Surfplatta iPad Pro 11",
                Type = ItemType.Elektronik,
                Status = ItemStatus.Trasig,
                Description = "Apple iPad Pro med Apple Pencil för grafiskt arbete eller inventering",
                Placement = "IT-skåpet",
                PurchaseDate = DateTime.Now.AddDays(-450)
            },
            new Item
            {
                Name = "Konferenstelefon (Spider)",
                Type = ItemType.Elektronik,
                Status = ItemStatus.Tillgänglig,
                Description = "Jabra Speak för kristallklart ljud i mindre sammanträdesrum",
                Placement = "Receptionens förråd",
                PurchaseDate = DateTime.Now.AddDays(-730)
            },
            new Item
            {
                Name = "Powerbank 20.000 mAh",
                Type = ItemType.Elektronik,
                Status = ItemStatus.Tillgänglig,
                Description = "Kraftfull nödladdare för mobiler och plattor vid heldagskonferenser",
                Placement = "IT-skåpet",
                PurchaseDate = DateTime.Now.AddDays(-60)
            },
            new Item
            {
                Name = "Bärbar Bildskärm 15\"",
                Type = ItemType.Elektronik,
                Status = ItemStatus.Tillgänglig,
                Description = "USB-C driven extraskärm för tillfälliga arbetsplatser",
                Placement = "IT-skåpet",
                PurchaseDate = DateTime.Now.AddDays(-200)
            },
            new Item
            {
                Name = "Webbkamera 4K",
                Type = ItemType.Elektronik,
                Status = ItemStatus.Tillgänglig,
                Description = "Logitech Brio för högkvalitativa videokonferenser och streaming",
                Placement = "Mediarummet Hylla A",
                PurchaseDate = DateTime.Now.AddDays(-310)
            },
            new Item
            {
                Name = "Brusreducerande Hörlurar",
                Type = ItemType.Elektronik,
                Status = ItemStatus.Tillgänglig,
                Description = "Sony WH-1000XM4 för koncentrerat arbete i öppna kontorslandskap",
                Placement = "IT-skåpet",
                PurchaseDate = DateTime.Now.AddDays(-180)
            },
            new Item
            {
                Name = "Digital systemkamera",
                Type = ItemType.Elektronik,
                Status = ItemStatus.Tillgänglig,
                Description = "Canon EOS med standardobjektiv för dokumentation och pressbilder",
                Placement = "Kommunikationsavdelningens skåp",
                PurchaseDate = DateTime.Now.AddDays(-500)
            },
            new Item
            {
                Name = "Diktafon Pro",
                Type = ItemType.Elektronik,
                Status = ItemStatus.Trasig,
                Description = "Olympus digital diktafon för inspelning av intervjuer och protokoll",
                Placement = "Kansliets arkiv",
                PurchaseDate = DateTime.Now.AddDays(-800)
            },
            new Item
            {
                Name = "Portabel Högtalare",
                Type = ItemType.Elektronik,
                Status = ItemStatus.Tillgänglig,
                Description = "Bose SoundLink för bakgrundsmusik eller mindre presentationer",
                Placement = "Receptionens förråd",
                PurchaseDate = DateTime.Now.AddDays(-40)
            },
            new Item
            {
                Name = "Lasermätare",
                Type = ItemType.Elektronik,
                Status = ItemStatus.Trasig,
                Description = "Bosch Professional för exakt mätning av lokaler och ytor",
                Placement = "Tekniska kontorets hylla",
                PurchaseDate = DateTime.Now.AddDays(-1100)
            },
            new Item
            {
                Name = "VR-Headset",
                Type = ItemType.Elektronik,
                Status = ItemStatus.Tillgänglig,
                Description = "Oculus Quest 2 för visualisering av stadsbyggnadsprojekt",
                Placement = "Innovationslabbet",
                PurchaseDate = DateTime.Now.AddDays(-250)
            },
            new Item
            {
                Name = "Adapterkit (All-in-one)",
                Type = ItemType.Elektronik,
                Status = ItemStatus.Tillgänglig,
                Description = "Väska med adaptrar för HDMI, VGA, DisplayPort och USB-C",
                Placement = "IT-skåpet",
                PurchaseDate = DateTime.Now.AddDays(-15)
            },
            new Item
            {
                Name = "Dokumentkamera",
                Type = ItemType.Elektronik,
                Status = ItemStatus.Tillgänglig,
                Description = "För att visa fysiska dokument eller objekt på storbild",
                Placement = "Utbildningsenhetens förråd",
                PurchaseDate = DateTime.Now.AddDays(-600)
            },
            new Item
            {
                Name = "Lamineringsmaskin A3",
                Type = ItemType.Elektronik,
                Status = ItemStatus.Trasig,
                Description = "För skyltning och skydd av informationsmaterial",
                Placement = "Kopieringsrummet",
                PurchaseDate = DateTime.Now.AddDays(-950)
            }
        };

        _context.Items.AddRange(dummyItems);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Lade till {dummyItems.Count} test-prylar i katalogen!" });
    }

    // Rensar hela katalogen på all data.
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearDatabase()
    {
        var allItems = await _context.Items.ToListAsync();

        _context.Items.RemoveRange(allItems);

        await _context.SaveChangesAsync();

        return Ok(new { message = "Katalogen är nu helt rensad!" });
    }
}