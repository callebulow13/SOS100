using KatalogApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace KatalogApi.Controllers;

// Adressen blir /api/items
[Route("api/[controller]")]
[ApiController]
public class ItemsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetItems()
    {
        // Testdata
        var fakeItems = new List<Item>
        {
            new Item { Id = 1, Name = "Bärbar Dator" },
            new Item { Id = 2, Name = "Kaffebryggare" },
            new Item { Id = 3, Name = "Kontorsstol" }
        };

        return Ok(fakeItems);
    }
}