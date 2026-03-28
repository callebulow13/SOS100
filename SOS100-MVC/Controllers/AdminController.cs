using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SOS100_MVC.Dtos;
using SOS100_MVC.Models;


namespace SOS100_MVC.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AdminController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult CreateUser()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(int id)
    {
        var client = _httpClientFactory.CreateClient("UserApi");
        var response = await client.GetAsync($"User/{id}");

        if (!response.IsSuccessStatusCode)
            return NotFound();

        var user = await response.Content.ReadFromJsonAsync<User>();
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(User user)
    {
        if (!ModelState.IsValid)
        {
            return View(user);
        }

        var client = _httpClientFactory.CreateClient("UserApi");
        var response = await client.PutAsJsonAsync($"User/{user.UserID}", user);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Kunde inte uppdatera användare");
            return View(user);
        }

        return RedirectToAction("Users");
    }

    public async Task<IActionResult> Users()
    {
        List<UserDto> users = new List<UserDto>();

        var client = _httpClientFactory.CreateClient("UserApi");
        var response = await client.GetAsync($"/User");

        if (response.IsSuccessStatusCode)
        {
            string data = await response.Content.ReadAsStringAsync();
            users = JsonSerializer.Deserialize<List<UserDto>>(data,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<UserDto>();
        }

        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(User user)
    {
        var json = JsonSerializer.Serialize(user);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var client = _httpClientFactory.CreateClient("UserApi");
        var response = await client.PostAsync("/User", content);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Users");
        }

        ModelState.AddModelError(string.Empty, "Something went wrong");
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var client = _httpClientFactory.CreateClient("UserApi");
        var response = await client.DeleteAsync($"User/{id}");

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Kunde inte radera användare");
            return RedirectToAction("Users");
        }

        return RedirectToAction("Users");
    }

    public async Task<IActionResult> SignOutUser()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

// GET: /Admin/Loans
    [HttpGet]
    public async Task<IActionResult> Loans()
    {
        var loans = new List<LoanDto>();
        try
        {
            var client = _httpClientFactory.CreateClient("LoanApi");

            // Hämta ALLA lån från LoanAPI
            var response = await client.GetAsync("/api/loans");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                loans = JsonSerializer.Deserialize<List<LoanDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<LoanDto>();
            }
            else
            {
                TempData["ErrorMessage"] = "Kunde inte hämta lånen från API:et.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Ett fel uppstod: {ex.Message}";
        }

        // Skickar listan till vyn Views/Admin/Loans.cshtml
        return View(loans);
    }

    // POST: /Admin/DeleteLoan
    [HttpPost]
    public async Task<IActionResult> DeleteLoan(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("LoanApi");

            // Gör ett DELETE-anrop till LoanAPI
            var response = await client.DeleteAsync($"/api/loans/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] =
                    "Lånet raderades permanent och prylen är nu tillgänglig (om lånet var aktivt).";
            }
            else
            {
                TempData["ErrorMessage"] = "Kunde inte radera lånet i API:et.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Ett nätverksfel uppstod: {ex.Message}";
        }

        return RedirectToAction("Loans");
    }

    // GET: /Admin/EditLoan/{id}
    [HttpGet]
    public async Task<IActionResult> EditLoan(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("LoanApi");

            // Hämta det specifika lånet från LoanAPI
            var response = await client.GetAsync($"/api/loans/{id}");

            if (response.IsSuccessStatusCode)
            {
                var loan = await response.Content.ReadFromJsonAsync<LoanDto>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (loan != null)
                {
                    return View(loan);
                }
            }

            TempData["ErrorMessage"] = "Kunde inte hämta lånet för redigering.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Ett nätverksfel uppstod: {ex.Message}";
        }

        return RedirectToAction("Loans");
    }

    // POST: /Admin/EditLoan
    [HttpPost]
    public async Task<IActionResult> EditLoan(Guid id, DateTime dueAt)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("LoanApi");

            // Vi skickar bara in det nya datumet till den slutpunkt vi skapade i LoanAPI:et
            var content = JsonContent.Create(dueAt);
            var response = await client.PutAsync($"/api/loans/{id}/due-date", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Lånets förfallodatum har uppdaterats!";
            }
            else
            {
                TempData["ErrorMessage"] = "Kunde inte uppdatera lånet i API:et.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Ett nätverksfel uppstod: {ex.Message}";
        }

        return RedirectToAction("Loans");
    }
}