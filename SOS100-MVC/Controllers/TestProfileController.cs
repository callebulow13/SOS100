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
}