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
    private readonly HttpClient _httpClient;
    
    public AdminController(IConfiguration configuration)
    {
        _httpClient = new  HttpClient();
        
        string apiBaseUrl = configuration.GetValue<string>("UserServiceBaseUrl");
        _httpClient.BaseAddress = new Uri(apiBaseUrl);
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
        
        var response = await _httpClient.GetAsync($"User/{id}");

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
        var response = await _httpClient.PutAsJsonAsync($"User/{user.UserID}", user);

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
        
        HttpResponseMessage response = await _httpClient.GetAsync($"/User");

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
        
        HttpResponseMessage response = await _httpClient.PostAsync("/User", content);

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
        var response = await _httpClient.DeleteAsync($"User/{id}");

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
}