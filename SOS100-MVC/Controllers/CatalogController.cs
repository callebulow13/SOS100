using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SOS100_MVC.Models;
using System.Security.Claims;

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
            throw new ArgumentNullException("KatalogApiBaseUrl",
                "Hittar ingen webbadress till API:et i inställningarna.");
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

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> ReportError(int ItemId, string ReporterName, string Description)
    {
        try
        {
            // 1. Skapa objektet för felanmälan som matchar databasens förväntningar
            var errorReport = new
            {
                ItemId = ItemId,
                ReporterName = ReporterName,
                Description = Description,
                ReportDate = DateTime.UtcNow,
                IsResolved = false
            };

            // 2. Skicka felanmälan via ett POST-anrop
            HttpResponseMessage reportResponse = await _httpClient.PostAsJsonAsync("/api/errorreports", errorReport);

            if (reportResponse.IsSuccessStatusCode)
            {
                // 3. EXTRA SÄKERHET: Hämta prylen från API:et och uppdatera status till Trasig
                // (Detta gör att din röda badge aktiveras direkt när sidan laddas om!)
                HttpResponseMessage itemResponse = await _httpClient.GetAsync($"/api/items/{ItemId}");

                if (itemResponse.IsSuccessStatusCode)
                {
                    string data = await itemResponse.Content.ReadAsStringAsync();
                    var item = JsonSerializer.Deserialize<Item>(data, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (item != null && item.Status != ItemStatus.Trasig)
                    {
                        // Ändra status till Trasig (vilket är 3 i din ItemStatus enum)
                        item.Status = ItemStatus.Trasig;

                        // Skicka PUT-anrop för att spara uppdateringen i databasen
                        await _httpClient.PutAsJsonAsync($"/api/items/{ItemId}", item);
                    }
                }

                // 4. Skicka tillbaka användaren till detaljvyn med ett fint meddelande
                TempData["ReportSuccess"] = "Tack för hjälpen! Din felanmälan har skickats till service teamet.";
                return RedirectToAction(nameof(Details), new { id = ItemId });
            }
            else
            {
                // Om API:et av någon anledning returnerar t.ex. 400 Bad Request
                TempData["ErrorMessage"] = "Kunde inte skicka felanmälan till servern.";
                return RedirectToAction(nameof(Details), new { id = ItemId });
            }
        }
        catch (Exception)
        {
            // Hanterar om API:et ligger nere eller inte kan nås
            TempData["ErrorMessage"] = "Ett oväntat nätverksfel uppstod vid felanmälan.";
            return RedirectToAction(nameof(Details), new { id = ItemId });
        }
    }

    // 1. Visar det tomma formuläret på skärmen
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // 2. Tar emot datan när användaren klickar på "Spara"
    [Authorize(Roles = "Admin")]
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

    // 1. Visar formuläret för att redigera en befintlig pryl
    [Authorize(Roles = "Admin")] // Bara administratörer får redigera
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        // Hämta prylen från API:et på samma sätt som vi gör i Details-vyn
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/items/{id}");

        if (response.IsSuccessStatusCode)
        {
            string data = await response.Content.ReadAsStringAsync();
            var item = JsonSerializer.Deserialize<Item>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (item != null)
            {
                // Skicka den befintliga datan till HTML-vyn så formuläret är förifyllt
                return View(item);
            }
        }

        return NotFound("Kunde inte hitta prylen för redigering.");
    }

    // 2. Tar emot datan när admin klickar på "Spara ändringar"
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Edit(Item updatedItem)
    {
        // Kolla så att admin inte har fyllt i något knasigt i formuläret
        if (!ModelState.IsValid)
        {
            return View(updatedItem);
        }

        // Gör ett PUT-anrop till API:et för att skriva över den gamla datan
        // Notera att vi skickar med updatedItem.Id i webbadressen, precis som vi testade tidigare!
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"/api/items/{updatedItem.Id}", updatedItem);

        if (response.IsSuccessStatusCode)
        {
            // Om API:et svarar 204 No Content (eller 200 OK), skicka tillbaka användaren till katalogen
            return RedirectToAction("Index");
        }

        // Om något gick fel
        ModelState.AddModelError("", "Kunde inte uppdatera objektet i API:et.");
        return View(updatedItem);
    }

    // 1. Visar bekräftelsesidan: "Är du säker på att du vill ta bort..."
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        // Vi måste hämta prylen så vi kan visa vad det är som håller på att raderas
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/items/{id}");

        if (response.IsSuccessStatusCode)
        {
            string data = await response.Content.ReadAsStringAsync();
            var item = JsonSerializer.Deserialize<Item>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (item != null)
            {
                return View(item);
            }
        }

        return NotFound("Kunde inte hitta prylen.");
    }

    // 2. Utför själva raderingen när admin klickar på "Ja, ta bort"
    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete")] // ActionName berättar att denna svarar på formuläret i Delete-vyn
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Skicka ett HTTP DELETE-anrop till ditt API
        HttpResponseMessage response = await _httpClient.DeleteAsync($"/api/items/{id}");

        if (response.IsSuccessStatusCode)
        {
            // Om API:et sa OK, skicka tillbaka admin till listan
            return RedirectToAction("Index");
        }

        // Om något gick fel
        return View("Error"); // Man kan skapa en specifik felsida här om man vill
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

    // POST: /Catalog/SeedCatalog
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> SeedCatalog()
    {
        // Eftersom _httpClient redan är uppsatt i din konstruktor gör vi bara ett snabbt anrop
        HttpResponseMessage response = await _httpClient.PostAsync("/api/items/seed", null);

        if (response.IsSuccessStatusCode)
            TempData["SuccessMessage"] = "Katalogen fylldes på med testdata!";
        else
            TempData["ErrorMessage"] = "Kunde inte fylla på katalogen. Har API:et seed-metoden?";

        return RedirectToAction("Index");
    }

    // POST: /Catalog/ClearCatalog
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> ClearCatalog()
    {
        // Samma sak här, vi använder den färdiga klienten
        HttpResponseMessage response = await _httpClient.DeleteAsync("/api/items/clear");

        if (response.IsSuccessStatusCode)
            TempData["SuccessMessage"] = "Katalogen är nu helt tom!";
        else
            TempData["ErrorMessage"] = "Kunde inte rensa katalogen. Har API:et clear-metoden?";

        return RedirectToAction("Index");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddWatch(int itemId, string itemTitle,
        [FromServices] IHttpClientFactory httpClientFactory, [FromServices] IConfiguration configuration)
    {
        // 1. Plocka ut ID på den inloggade användaren
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int userId))
        {
            TempData["ErrorMessage"] = "Kunde inte identifiera ditt användarkonto.";
            return RedirectToAction("Index");
        }

        // 2. Skapa objektet som kompisens API förväntar sig (baserat på Watch.cs)
        var newWatch = new
        {
            UserId = userId,
            ItemId = itemId,
            ItemTitle = itemTitle,
            IsActive = true
        };

        // 3. Skicka anropet till ReminderApi
        try
        {
            var client = httpClientFactory.CreateClient();

// Vi kollar efter BÅDA namnen för säkerhets skull!
            var baseUrl = configuration["ReminderApiBaseUrl"] ??
                          configuration["ReminderServiceBaseUrl"] ?? "http://localhost:5038";

// Vi kollar efter API-nyckeln (och lägger in samma default som i din ReminderServiceClient)
            var apiKey = configuration["ReminderApiKey"] ?? "reminder-hemlig-123";

            client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

            var response = await client.PostAsJsonAsync($"{baseUrl}/api/watches", newWatch);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Du bevakar nu {itemTitle}!";
            }
            else
            {
                // Läs av det faktiska svaret från kompisens API
                var errorDetails = await response.Content.ReadAsStringAsync();

                // Visa HTTP-statuskoden och API:ets eget felmeddelande på skärmen
                TempData["ErrorMessage"] = $"API-fel ({response.StatusCode}): {errorDetails}";
            }
        }
        catch
        {
            TempData["ErrorMessage"] = "Kunde inte nå bevakningstjänsten just nu.";
        }

        return RedirectToAction("Index");
    }
}