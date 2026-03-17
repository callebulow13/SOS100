using Microsoft.AspNetCore.Mvc;
using SOS100_MVC.Models;
using System.Security.Claims;
using SOS100_MVC.Dtos;

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

    public async Task<IActionResult> Index()
    {
        // 1. LÄS AV VEM SOM ÄR INLOGGAD
        var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Om ingen är inloggad, avbryt och visa ett fel (eller skicka till startsidan)
        if (string.IsNullOrEmpty(loggedInUserId))
        {
            return Content("Du måste vara inloggad för att se denna sida.");
        }

        var client = _httpClientFactory.CreateClient();

        // 2. HÄMTA ANVÄNDAREN
        var userBaseUrl = _configuration["UserServiceBaseUrl"];
        // Vi skickar in det inloggade ID:t till kompisens API
        var userResponse = await client.GetAsync($"{userBaseUrl}/User/{loggedInUserId}");

        if (!userResponse.IsSuccessStatusCode)
            return Content($"Kunde inte hämta användare med ID {loggedInUserId}.");

        var user = await userResponse.Content.ReadFromJsonAsync<User>();

        // 3. HÄMTA LÅNEN
        var loanBaseUrl = _configuration["LoanServiceBaseUrl"] ?? "http://localhost:5125";

        var loanResponse = await client.GetAsync($"{loanBaseUrl}/api/loans");
        var myActiveLoans = new List<LoanDto>();

        if (loanResponse.IsSuccessStatusCode)
        {
            var allLoans = await loanResponse.Content.ReadFromJsonAsync<List<LoanDto>>();
            if (allLoans != null)
            {
                // Vi filtrerar fram lånen för den inloggade användaren
                myActiveLoans = allLoans.Where(l =>
                    l.ReturnedAt == null &&
                    (l.BorrowerId == user.UserID.ToString() || l.BorrowerId == user.Username)
                ).ToList();
            }

            ViewBag.DebugMessage =
                $"Hämtade {allLoans?.Count ?? 0} lån totalt. Filtrerade fram {myActiveLoans.Count} st lån för {user.Username}.";
        }

        // 4. LÄGG ALLT I KORGEN (ViewModel)
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

        // ── Steg 1: Återlämna lånet ──
        var response = await client.PostAsync(
            $"{loanBaseUrl}/api/loans/{loanId}/return", null);

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = $"Kunde inte återlämna. API svarade: {errorText}";
            return RedirectToAction("Index", new { id = userId });
        }

        // ── Steg 2: Hitta reminder för detta lån och markera som skickad ──
        try
        {
            var reminderBaseUrl = _configuration["ReminderServiceBaseUrl"] ?? "http://localhost:5038";
            var reminderApiKey = _configuration["ReminderApiKey"] ?? "reminder-hemlig-123";

            var reminderClient = _httpClientFactory.CreateClient();
            reminderClient.DefaultRequestHeaders.Add("X-Api-Key", reminderApiKey);

            // Hämta alla reminders
            var remindersResponse = await reminderClient.GetAsync(
                $"{reminderBaseUrl}/api/reminders");

            if (remindersResponse.IsSuccessStatusCode)
            {
                var reminders = await remindersResponse.Content
                    .ReadFromJsonAsync<List<ReminderDto>>();

                // Hitta reminder som matchar detta lån (loanId som sträng)
                var match = reminders?.FirstOrDefault(r =>
                    r.LoanId == loanId.ToString() ||
                    r.UserId == userId.ToString());

                if (match != null)
                {
                    // Markera som skickad
                    var updated = new
                    {
                        isSent = true,
                        dueDate = match.DueDate,
                        itemTitle = match.ItemTitle
                    };

                    await reminderClient.PutAsJsonAsync(
                        $"{reminderBaseUrl}/api/reminders/{match.Id}", updated);

                    Console.WriteLine($"✅ Reminder {match.Id} markerad som skickad!");
                }
            }
        }
        catch (Exception ex)
        {
            // Logga men avbryt inte — lånet är redan återlämnat
            Console.WriteLine($"⚠️ Kunde inte uppdatera reminder: {ex.Message}");
        }

        return RedirectToAction("Index", new { id = userId });
    }
}