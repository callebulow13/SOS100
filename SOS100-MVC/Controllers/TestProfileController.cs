using Microsoft.AspNetCore.Mvc;
using SOS100_MVC.Models; // För att kunna använda User-klassen

namespace SOS100_MVC.Controllers;

public class TestProfileController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    // Vi berövar in de verktyg din kompis använder för API-anrop
    public TestProfileController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    // Genom att lägga till "int id = 1" hämtar vi automatiskt användare 1 om inget annat anges
    public async Task<IActionResult> Index(int id = 1)
    {
        var client = _httpClientFactory.CreateClient();
        var baseUrl = _configuration["UserServiceBaseUrl"];

        // Gör anropet till din kompis API för att hämta den specifika användaren
        var response = await client.GetAsync($"{baseUrl}/User/{id}");

        if (!response.IsSuccessStatusCode) 
        {
            return Content($"Kunde inte hämta användare med ID {id} från API:et. Är UserService igång?");
        }

        var user = await response.Content.ReadFromJsonAsync<User>();

        // Skicka den RIKTIGA användaren till din test-vy
        return View(user);
    }
}