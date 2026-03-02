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
    public AccountController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    public IActionResult Index(string returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(Account account, string returnUrl)
    {
        var loginDto = new
        {
            Username = account.Username,
            Password = account.Password,
        };
        
        var client = _httpClientFactory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "http://localhost:5196/user/login",
            loginDto);
        
        //Fel användarnamn eller lösenord
        if (!response.IsSuccessStatusCode)
        {
            ViewBag.ErrorMessage = "Login failed: Wrong username or password";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        var apiUser = await response.Content.ReadFromJsonAsync<ApiUserDto>();
        
        //Korrekt användarnamn och lösenord
        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        
        identity.AddClaim(new Claim(ClaimTypes.Name, apiUser.Username));
        identity.AddClaim(new Claim(ClaimTypes.Role, apiUser.Role));
        
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
        
        //Ifall ingen returnUrl, gå till Home
        if (String.IsNullOrEmpty(returnUrl))
        {
            return RedirectToAction("Index", "Home");
        }
        
        //Gå tillbaka via returnUrl
        return Redirect(returnUrl);
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

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        if (string.IsNullOrEmpty(returnUrl))
            return RedirectToAction("Index", "Home");

        return Redirect(returnUrl);
    }
}