using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SOS100_MVC.Models;

namespace SOS100_MVC.Controllers;
[AllowAnonymous]
public class AccountController : Controller
{
    public IActionResult Index(string returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(Account account, string returnUrl)
    {
        //Roll för att avgöra admin eller user
        string role = null;
        
        //Kolla användarnamn och lösenord
        if (account.Username == "admin" && account.Password == "abc123")
        {
            role = "Admin";
        }
        else if (account.Username == "user" && account.Password == "123456")
        {
            role = "User";
        }
        
        //Fel användarnamn eller lösenord
        if (role == null)
        {
            ViewBag.ErrorMessage = "Login failed: Wrong username or password";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        
        //Korrekt användarnamn och lösenord
        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        
        identity.AddClaim(new Claim(ClaimTypes.Name, account.Username));
        identity.AddClaim(new Claim(ClaimTypes.Role, role));
        
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