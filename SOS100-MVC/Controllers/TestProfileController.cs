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
        
        // 1. HÄMTA ANVÄNDAREN
        User? user = null;
        try
        {
            var userBaseUrl = _configuration["UserServiceBaseUrl"];
            var userResponse = await client.GetAsync($"{userBaseUrl}/User/{id}");
            if (userResponse.IsSuccessStatusCode)
                user = await userResponse.Content.ReadFromJsonAsync<User>();
        }
        catch
        {
            // UserService är nere
        }

        if (user == null)
            return Content($"Kunde inte hämta användare med ID {id}. UserService kanske inte körs.");

        // 2. HÄMTA LÅNEN
        var myActiveLoans = new List<LoanDto>();
        try
        {
            var loanBaseUrl = _configuration["LoanServiceBaseUrl"] ?? "http://localhost:5125";
            var loanResponse = await client.GetAsync($"{loanBaseUrl}/api/loans");
            
            if (loanResponse.IsSuccessStatusCode)
            {
                var allLoans = await loanResponse.Content.ReadFromJsonAsync<List<LoanDto>>();
                if (allLoans != null)
                {
                    myActiveLoans = allLoans.Where(l => 
                        l.ReturnedAt == null && 
                        (l.BorrowerId == user.UserID.ToString() || l.BorrowerId == user.Username)
                    ).ToList();
                }
                ViewBag.DebugMessage = $"Hämtade {allLoans?.Count ?? 0} lån totalt. Filtrerade fram {myActiveLoans.Count} st lån för {user.Username}.";
            }
        }
        catch
        {
            // LoanService är nere
            ViewBag.DebugMessage = "LoanService är inte tillgänglig just nu.";
        }

        // 3. BYGG VIEWMODEL
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
        try
        {
            var client = _httpClientFactory.CreateClient();
            var loanBaseUrl = _configuration["LoanServiceBaseUrl"] ?? "http://localhost:5125";
            var response = await client.PostAsync($"{loanBaseUrl}/api/loans/{loanId}/return", null);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Kunde inte återlämna. API svarade: {errorText}";
            }
        }
        catch
        {
            TempData["ErrorMessage"] = "Kunde inte nå LoanService.";
        }

        return RedirectToAction("Index", new { id = userId });
    }
}