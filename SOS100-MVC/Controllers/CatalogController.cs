using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SOS100_MVC.Models;

namespace SOS100_MVC.Controllers;

public class CatalogController : Controller
{
    private readonly HttpClient _httpClient;

    // Konstruktor
    public CatalogController(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        
        // Hämta URL från appsettings
        string apiBaseUrl = configuration.GetValue<string>("KatalogApiBaseUrl");
        if (string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            throw new ArgumentNullException("KatalogApiBaseUrl", "Hittar ingen webbadress till API:et i inställningarna.");
        }
        
        _httpClient.BaseAddress = new Uri(apiBaseUrl); 
        
        // Hämta API-nyckeln från appsettings
        string apiKey = configuration.GetValue<string>("KatalogApiKey");
        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
    }


    
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        // 1. Gör anropet för att hämta just den specifika prylen.
        // Eftersom _httpClient redan har BaseAddress och API-nyckel, behöver vi bara skicka in /api/items/{id}
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/items/{id}");

        if (response.IsSuccessStatusCode)
        {
            // 2. Läs svaret och packa upp JSON-datan
            string data = await response.Content.ReadAsStringAsync();
            var item = JsonSerializer.Deserialize<Item>(data, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            // 3. Om prylen inte finns (trots att API:et svarade OK)
            if (item == null)
            {
                return NotFound("Kunde inte tolka datan för prylen.");
            }

            // 4. Skicka prylen till vår nya HTML-vy!
            return View(item);
        }

        // Om API:et returnerar 404 Not Found
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound("Kunde inte hitta någon pryl med det ID:t.");
        }

        // Om något annat går fel (t.ex. 500 Internal Server Error)
        return View("Error");
    }
    // 1. Visar det tomma formuläret på skärmen
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // 2. Tar emot datan när användaren klickar på "Spara"
    [HttpPost]
    public async Task<IActionResult> Create(Item newItem)
    {
        // Kontrollera så att användaren har fyllt i alla obligatoriska fält korrekt
        if (!ModelState.IsValid)
        {
            return View(newItem);
        }

        // Vi sätter alltid status till "Tillgänglig" (0) när en pryl skapas, 
        // för man kan ju inte lägga till en pryl som redan är utlånad!
        newItem.Status = ItemStatus.Tillgänglig;

        // Om pryl-ID sätts automatiskt av databasen är det bra att rensa det
        newItem.Id = 0; 

        // Gör ett POST-anrop till ditt KatalogApi med den nya prylen
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/items", newItem);

        if (response.IsSuccessStatusCode)
        {
            // Om API:et sa OK (201 Created), skicka tillbaka användaren till listan
            return RedirectToAction("Index");
        }

        // Om något gick snett i API:et (t.ex. valideringsfel)
        ModelState.AddModelError("", "Kunde inte spara objektet i API:et. Kontrollera uppgifterna.");
        return View(newItem);
    }

    public async Task<IActionResult> Index()
    {
        List<Item> items = new List<Item>();

        // Vi hämtar alltid alla prylar
        HttpResponseMessage response = await _httpClient.GetAsync("/api/items");

        if (response.IsSuccessStatusCode)
        {
            string data = await response.Content.ReadAsStringAsync();
            items = JsonSerializer.Deserialize<List<Item>>(data, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
        }
        
        return View(items);
    }
}