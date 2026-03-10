using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SOS100_MVC.Dtos;
using SOS100_MVC.Models;

namespace SOS100_MVC.Controllers;
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    
    public AccountController(IHttpClientFactory httpClientFactory,  IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }
    
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(Account account)
    {
        var loginDto = new
        {
            Username = account.Username,
            Password = account.Password,
        };
        
        var client = _httpClientFactory.CreateClient();
        var baseUrl = _configuration["UserServiceBaseUrl"];

        var response = await client.PostAsJsonAsync(
            $"{baseUrl}/User/Login",
            loginDto);
        
        //Fel användarnamn eller lösenord
        if (!response.IsSuccessStatusCode)
        {
            ViewBag.ErrorMessage = "Login failed: Wrong username or password";
            return View();
        }

        var apiUser = await response.Content.ReadFromJsonAsync<ApiUserDto>();
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, apiUser.UserID.ToString()),
            new Claim(ClaimTypes.Name, apiUser.Username),
            new Claim(ClaimTypes.Role, apiUser.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(principal));
        
        //Gå tillbaka via returnUrl
        return Redirect($"Home/Index/");
    }
    
    public async Task<IActionResult> SignOutUser()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
    
    
    //Logga in snabbt via knapp under produktion
    [HttpPost]
    public async Task<IActionResult> QuickAdminLogin(string returnUrl)
    {
        var identity = new ClaimsIdentity(
            CookieAuthenticationDefaults.AuthenticationScheme);

        identity.AddClaim(new Claim(ClaimTypes.Name, "admin"));
        identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "1"));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        if (string.IsNullOrEmpty(returnUrl))
            return RedirectToAction("Index", "Home");

        return Redirect(returnUrl);
    }

    [HttpPost]
    public async Task<IActionResult> QuickUserLogin(string returnUrl)
    {
        var identity = new ClaimsIdentity(
            CookieAuthenticationDefaults.AuthenticationScheme);

        identity.AddClaim(new Claim(ClaimTypes.Name, "user"));
        identity.AddClaim(new Claim(ClaimTypes.Role, "User"));
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "2"));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        if (string.IsNullOrEmpty(returnUrl))
            return RedirectToAction("Index", "Home");

        return Redirect(returnUrl);
    }
}