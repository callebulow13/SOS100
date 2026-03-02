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

    // Ändrade namnet till Index() för snyggare webbadress
    public async Task<IActionResult> Index()
    {
        List<Item> items = new List<Item>();

        // Gör anropet till API:et
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