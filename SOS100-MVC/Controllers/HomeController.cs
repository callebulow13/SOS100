using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SOS100_MVC.Models;

namespace SOS100_MVC.Controllers;

public class HomeController : Controller
{
    private readonly HttpClient _httpClient;
    // Konstruktor
    public HomeController(IConfiguration configuration)
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
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    // --- KATALOGEN ---
    public async Task<IActionResult> Catalog()
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
        // Returnerar vyn "Catalog" och skickar med listan
        return View(items);
    }
}