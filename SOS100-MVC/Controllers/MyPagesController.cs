using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SOS100_MVC.Models;

namespace SOS100_MVC.Controllers;

public class MyPagesController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public MyPagesController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
    
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
                return Content("UserId missing");

        var client = _httpClientFactory.CreateClient();
        var baseUrl = _configuration["UserServiceBaseUrl"];

        var response = await client.GetAsync($"{baseUrl}/User/{userId}");

        if (!response.IsSuccessStatusCode) 
            return Content("API call failed");

        var user = await response.Content.ReadFromJsonAsync<User>();

        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var client = _httpClientFactory.CreateClient();
        var baseUrl = _configuration["UserServiceBaseUrl"];
        
        var response = await client.GetAsync($"{baseUrl}/User/{userId}");
        
        if (!response.IsSuccessStatusCode)
            return Content("Kan inte visa användare");
        
        var user = await response.Content.ReadFromJsonAsync<User>();
        return View(user);
    }
    
    [HttpPost]
    public async Task<IActionResult> EditProfile(User user)
    {
        var client = _httpClientFactory.CreateClient();
        var baseUrl = _configuration["UserServiceBaseUrl"];
        
        client.DefaultRequestHeaders.Add("Cookie", Request.Headers["Cookie"].ToString());
        
        var response = await client.PutAsJsonAsync($"{baseUrl}/User/profile/{user.UserID}", user);

        if (!response.IsSuccessStatusCode)
            return Content("Update failed");

        return RedirectToAction("Index");
    }
}