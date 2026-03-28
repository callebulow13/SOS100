using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SOS100_MVC.Models;
using System.Security.Claims;

namespace SOS100_MVC.Controllers;

public class CatalogController : Controller
{
    private readonly HttpClient _httpClient;

    public CatalogController(IConfiguration configuration)
    {
        // Denna klient används för att göra HTTP-anrop.
        _httpClient = new HttpClient();

        // Hämtar URL:en till API från inställningarna
        string apiBaseUrl = configuration.GetValue<string>("KatalogApiBaseUrl");
        if (string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            throw new ArgumentNullException("KatalogApiBaseUrl",
                "Hittar ingen webbadress till API:et i inställningarna.");
        }

        // Sätter grund-adressen. Alla anrop i denna controller kommer börja med denna URL.
        _httpClient.BaseAddress = new Uri(apiBaseUrl);

        // Hämtar och bifogar API-nyckeln i Headern för alla anrop.
        string apiKey = configuration.GetValue<string>("KatalogApiKey");
        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
    }


    // Hämtar och visar detaljer för ett specifikt objekt
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        // Gör ett GET-anrop till API:et.
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/items/{id}");

        if (response.IsSuccessStatusCode)
        {
            // Läs svaret som en sträng (oftast JSON).
            string data = await response.Content.ReadAsStringAsync();
            // Omvandla (deserialisera) JSON-strängen tillbaka till ett C#-objekt (Item).
            var item = JsonSerializer.Deserialize<Item>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (item == null)
            {
                return NotFound("Kunde inte tolka datan för prylen.");
            }

            // Skicka objektet till vyn (Details.cshtml).
            return View(item);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound("Kunde inte hitta någon pryl med det ID:t.");
        }

        return View("Error");
    }

    // Skapar en felrapport för ett specifik objekt.
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> ReportError(int ItemId, string ReporterName, string Description)
    {
        try
        {
            var errorReport = new
            {
                ItemId = ItemId,
                ReporterName = ReporterName,
                Description = Description,
                ReportDate = DateTime.UtcNow,
                IsResolved = false
            };

            // Skickar rapporten till ErrorReports-slutpunkten i API:et.
            HttpResponseMessage reportResponse = await _httpClient.PostAsJsonAsync("/api/errorreports", errorReport);

            if (reportResponse.IsSuccessStatusCode)
            {
                HttpResponseMessage itemResponse = await _httpClient.GetAsync($"/api/items/{ItemId}");

                if (itemResponse.IsSuccessStatusCode)
                {
                    string data = await itemResponse.Content.ReadAsStringAsync();
                    var item = JsonSerializer.Deserialize<Item>(data, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // Uppdatera statusen på objektet till "Trasig"
                    if (item != null && item.Status != ItemStatus.Trasig)
                    {
                        item.Status = ItemStatus.Trasig;

                        await _httpClient.PutAsJsonAsync($"/api/items/{ItemId}", item);
                    }
                }

                TempData["ReportSuccess"] = "Tack för hjälpen! Din felanmälan har skickats till service teamet.";
                return RedirectToAction(nameof(Details), new { id = ItemId });
            }
            else
            {
                TempData["ErrorMessage"] = "Kunde inte skicka felanmälan till servern.";
                return RedirectToAction(nameof(Details), new { id = ItemId });
            }
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Ett oväntat nätverksfel uppstod vid felanmälan.";
            return RedirectToAction(nameof(Details), new { id = ItemId });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }
    
    // Skapa objekt 
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(Item newItem)
    {
        // Kollar om formulärdatan är giltig (t.ex. att inga obligatoriska fält saknas).
        if (!ModelState.IsValid)
        {
            return View(newItem);
        }

        newItem.Status = ItemStatus.Tillgänglig;

        // Säkerställer att API:et skapar ett nytt ID
        newItem.Id = 0;

        // Skickar det nya objektet som JSON till API:et.   
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/items", newItem);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Index");
        }

        ModelState.AddModelError("", "Kunde inte spara objektet i API:et. Kontrollera uppgifterna.");
        return View(newItem);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        // Hämtar det aktuella objektet för att fylla i redigeringsformuläret.
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

        return NotFound("Kunde inte hitta prylen för redigering.");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Edit(Item updatedItem)
    {
        if (!ModelState.IsValid)
        {
            return View(updatedItem);
        }

        // Skickar en PUT-förfrågan för att uppdatera objektet.
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"/api/items/{updatedItem.Id}", updatedItem);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Index");
        }

        ModelState.AddModelError("", "Kunde inte uppdatera objektet i API:et.");
        return View(updatedItem);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        // Hämtar objektet som ska tas bort så vi kan visa en bekräftelseruta för användaren.
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

    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete")] 
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Skickar en DELETE-förfrågan till API:et.
        HttpResponseMessage response = await _httpClient.DeleteAsync($"/api/items/{id}");

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Index");
        }

        return View("Error");
    }

    /// Hämtar hela katalogen och visar den på förstasidan (Index).
    public async Task<IActionResult> Index()
    {
        List<Item> items = new List<Item>();

        // Anropar /api/items för att få en lista på alla objekt
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

    // --- Metoder för att underlätta testning och utveckling ---
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> SeedCatalog()
    {
        HttpResponseMessage response = await _httpClient.PostAsync("/api/items/seed", null);

        if (response.IsSuccessStatusCode)
            TempData["SuccessMessage"] = "Katalogen fylldes på med testdata!";
        else
            TempData["ErrorMessage"] = "Kunde inte fylla på katalogen. Har API:et seed-metoden?";

        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> ClearCatalog()
    {
        HttpResponseMessage response = await _httpClient.DeleteAsync("/api/items/clear");

        if (response.IsSuccessStatusCode)
            TempData["SuccessMessage"] = "Katalogen är nu helt tom!";
        else
            TempData["ErrorMessage"] = "Kunde inte rensa katalogen. Har API:et clear-metoden?";

        return RedirectToAction("Index");
    }

    // Lägger till en bevakning på ett utlånat objekt.
    // Denna metod kontaktar en HELT ANNAN mikrotjänst (ReminderService).
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddWatch(int itemId, string itemTitle,
        [FromServices] IHttpClientFactory httpClientFactory, [FromServices] IConfiguration configuration)
    {
        // 1. Hämtar det inloggade användarens ID från inloggnings-cookien.
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int userId))
        {
            TempData["ErrorMessage"] = "Kunde inte identifiera ditt användarkonto.";
            return RedirectToAction("Index");
        }

        // 2. Förbereder bevaknings-objektet.
        var newWatch = new
        {
            UserId = userId,
            ItemId = itemId,
            ItemTitle = itemTitle,
            IsActive = true
        };

        try
        {
            // 3. Skapar en ny tillfällig HttpClient (eftersom vi ska till en annan tjänst).
            var client = httpClientFactory.CreateClient();

            // 4. Hämtar URL och Nyckel för ReminderService.
            var baseUrl = configuration["ReminderApiBaseUrl"] ??
                          configuration["ReminderServiceBaseUrl"] ?? "http://localhost:5038";

            var apiKey = configuration["ReminderApiKey"] ?? "reminder-hemlig-123";

            client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

            // 5. Skickar anropet för att skapa bevakningen.
            var response = await client.PostAsJsonAsync($"{baseUrl}/api/watches", newWatch);

            if (response.IsSuccessStatusCode)
            {
                // TempData skickar med ett kortlivat meddelande till nästa sida som laddas.
                TempData["SuccessMessage"] = $"Du bevakar nu {itemTitle}!";
            }
            else
            {
                var errorDetails = await response.Content.ReadAsStringAsync();

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