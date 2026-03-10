using Microsoft.AspNetCore.Mvc;
using SOS100_MVC.Services;

namespace SOS100_MVC.Controllers;

public class MyPagesController : Controller
{
    private readonly ReminderServiceClient _reminderService;

    public MyPagesController(ReminderServiceClient reminderService)
    {
        _reminderService = reminderService;
    }

    public async Task<IActionResult> Index()
    {
        int userId = 1;

        var reminders = await _reminderService.GetRemindersAsync(userId);
        var watches = await _reminderService.GetWatchesAsync(userId);
        var overdueCount = await _reminderService.GetOverdueCountAsync();

        ViewBag.Reminders = reminders;
        ViewBag.Watches = watches;
        ViewBag.OverdueCount = overdueCount;

        return View();
    }
}