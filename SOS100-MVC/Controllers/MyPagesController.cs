using Microsoft.AspNetCore.Mvc;

namespace SOS100_MVC.Controllers;

public class MyPagesController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}