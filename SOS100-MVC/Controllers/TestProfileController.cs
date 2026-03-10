using Microsoft.AspNetCore.Mvc;
using SOS100_MVC.Models;

namespace SOS100_MVC.Controllers;

public class TestProfileController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public TestProfileController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index(int id = 1)
    {
        var client = _httpClientFactory.CreateClient();
        
        // 1. HÄMTA ANVÄNDAREN (Från din UserService)
        var userBaseUrl = _configuration["UserServiceBaseUrl"];
        var userResponse = await client.GetAsync($"{userBaseUrl}/User/{id}");

        if (!userResponse.IsSuccessStatusCode) 
            return Content($"Kunde inte hämta användare med ID {id}.");

        var user = await userResponse.Content.ReadFromJsonAsync<User>();

// 2. HÄMTA LÅNEN
        var loanBaseUrl = _configuration["LoanServiceBaseUrl"] ?? "http://localhost:5125"; // Din kompis port
        
        // NYTT TRICK: Vi hämtar ALLA lån utan att ange någon status. 
        // Då kringgår vi kraschen i kompisens API helt och hållet!
        // Den ska sluta precis efter "loans"
        var loanResponse = await client.GetAsync($"{loanBaseUrl}/api/loans");
        
        var myActiveLoans = new List<LoanDto>();
        
        if (loanResponse.IsSuccessStatusCode)
        {
            var allLoans = await loanResponse.Content.ReadFromJsonAsync<List<LoanDto>>();
            if (allLoans != null)
            {
                // NYTT: Nu kollar vi BARA mot den specifika användarens ID eller Användarnamn.
                // Inga hårdkodade "admin"-undantag kvar!
                myActiveLoans = allLoans.Where(l => 
                    l.ReturnedAt == null && 
                    (l.BorrowerId == user.UserID.ToString() || l.BorrowerId == user.Username)
                ).ToList();
            }
            
            ViewBag.DebugMessage = $"Hämtade {allLoans?.Count ?? 0} lån totalt. Filtrerade fram {myActiveLoans.Count} st lån för {user.Username}.";
        }

        // 3. LÄGG ALLT I KORGEN (ViewModel)
        var viewModel = new ProfileViewModel
        {
            User = user,
            ActiveLoans = myActiveLoans
        };

        return View(viewModel);
    }
    [HttpPost]
    public async Task<IActionResult> ReturnItem(Guid loanId, int userId)
    {
        var client = _httpClientFactory.CreateClient();
        var loanBaseUrl = _configuration["LoanServiceBaseUrl"] ?? "http://localhost:5125";

        // Kompisens API väntar sig ett POST-anrop till adressen: /api/loans/{loanId}/return
        // Eftersom vi inte behöver skicka med någon data i själva "kroppen" av anropet, skickar vi "null".
        var response = await client.PostAsync($"{loanBaseUrl}/api/loans/{loanId}/return", null);

        if (response.IsSuccessStatusCode)
        {
            // Om API:et returnerade Ok (200), ladda om profilsidan för samma användare!
            return RedirectToAction("Index", new { id = userId });
        }
        else
        {
            // Om något gick fel, läser vi felmeddelandet och skickar med det till vyn via TempData
            var errorText = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = $"Kunde inte återlämna. API svarade: {errorText}";
            
            return RedirectToAction("Index", new { id = userId });
        }
    }
}