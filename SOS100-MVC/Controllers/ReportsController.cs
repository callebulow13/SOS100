using Microsoft.AspNetCore.Mvc;
using SOS100_MVC.Models.Reports;
using SOS100_MVC.Services;

namespace SOS100_MVC.Controllers;

public class ReportsController : Controller
{
    private readonly ReportApiService _reportApiService;

    public ReportsController(ReportApiService reportApiService)
    {
        _reportApiService = reportApiService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new ReportsPageViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Index(ReportsPageViewModel model)
    {
        switch (model.SelectedReport)
        {
            case "most-loaned":
                model.MostLoanedItems = await _reportApiService
                    .GetMostLoanedItemsAsync(model.MostLoanedLimit);
                break;

            case "overdue":
                model.OverdueLoanCount = await _reportApiService.GetOverdueLoansCountAsync();
                break;

            case "item-history":
            {
                bool hasItemId = model.ItemId.HasValue;
                bool hasItemName = !string.IsNullOrWhiteSpace(model.ItemName);

                if (!hasItemId && !hasItemName)
                {
                    ModelState.AddModelError("", "Fyll i antingen objekt-ID eller objektnamn.");
                    return View(model);
                }

                if (hasItemId && hasItemName)
                {
                    ModelState.AddModelError("", "Fyll i antingen objekt-ID eller objektnamn, inte båda.");
                    return View(model);
                }

                if (hasItemId)
                {
                    model.ItemLoanHistory = await _reportApiService
                        .GetItemLoanHistoryAsync(model.ItemId.Value);
                }
                else
                {
                    model.ItemLoanHistory = await _reportApiService
                        .GetItemLoanHistoryByNameAsync(model.ItemName!.Trim());
                }

                break;
            }

            case "user-history":
            {
                bool hasUserId = model.UserId.HasValue;
                bool hasUserName = !string.IsNullOrWhiteSpace(model.UserName);

                if (!hasUserId && !hasUserName)
                {
                    ModelState.AddModelError("", "Fyll i antingen användar-ID eller användarnamn.");
                    return View(model);
                }

                if (hasUserId && hasUserName)
                {
                    ModelState.AddModelError("", "Fyll i antingen användar-ID eller användarnamn, inte båda.");
                    return View(model);
                }

                if (hasUserId)
                {
                    model.UserLoanHistory = await _reportApiService
                        .GetUserLoanHistoryAsync(model.UserId.Value);
                }
                else
                {
                    model.UserLoanHistory = await _reportApiService
                        .GetUserLoanHistoryByNameAsync(model.UserName!.Trim());
                }

                break;
            }
            case "current-loaned":
                model.CurrentLoanedItems = await _reportApiService.GetCurrentLoanedItemsAsync();
                break;
        }

        return View(model);
    }
}