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
    public async Task<IActionResult> Index()
    {
        var model = new ReportsPageViewModel
        {
            SavedReports = await _reportApiService.GetSavedReportsAsync()
        };

        return View(model);
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

        model.SavedReports = await _reportApiService.GetSavedReportsAsync();
        return View(model);
    }
    
    [HttpPost]
    public async Task<IActionResult> SaveReport(ReportsPageViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.SavedReportName))
        {
            ModelState.AddModelError("", "Ange ett namn för rapporten.");
            model.SavedReports = await _reportApiService.GetSavedReportsAsync();
            return View("Index", model);
        }

        var createModel = new CreateSavedReportViewModel
        {
            Name = model.SavedReportName.Trim(),
            ReportType = model.SelectedReport ?? string.Empty,
            ItemId = model.ItemId,
            ItemName = model.ItemName,
            UserId = model.UserId,
            UserName = model.UserName,
            MostLoanedLimit = model.MostLoanedLimit
        };

        await _reportApiService.CreateSavedReportAsync(createModel);

        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    public async Task<IActionResult> OpenSavedReport(int id)
    {
        var saved = await _reportApiService.GetSavedReportByIdAsync(id);

        if (saved == null)
            return RedirectToAction(nameof(Index));

        var model = new ReportsPageViewModel
        {
            SelectedReport = saved.ReportType,
            ItemId = saved.ItemId,
            ItemName = saved.ItemName,
            UserId = saved.UserId,
            UserName = saved.UserName,
            MostLoanedLimit = saved.MostLoanedLimit,
            SavedReports = await _reportApiService.GetSavedReportsAsync()
        };

        switch (model.SelectedReport)
        {
            case "most-loaned":
                model.MostLoanedItems = await _reportApiService.GetMostLoanedItemsAsync(model.MostLoanedLimit);
                break;

            case "overdue":
                model.OverdueLoanCount = await _reportApiService.GetOverdueLoansCountAsync();
                break;

            case "item-history":
                if (model.ItemId.HasValue)
                    model.ItemLoanHistory = await _reportApiService.GetItemLoanHistoryAsync(model.ItemId.Value);
                else if (!string.IsNullOrWhiteSpace(model.ItemName))
                    model.ItemLoanHistory = await _reportApiService.GetItemLoanHistoryByNameAsync(model.ItemName);
                break;

            case "user-history":
                if (model.UserId.HasValue)
                    model.UserLoanHistory = await _reportApiService.GetUserLoanHistoryAsync(model.UserId.Value);
                else if (!string.IsNullOrWhiteSpace(model.UserName))
                    model.UserLoanHistory = await _reportApiService.GetUserLoanHistoryByNameAsync(model.UserName);
                break;

            case "current-loaned":
                model.CurrentLoanedItems = await _reportApiService.GetCurrentLoanedItemsAsync();
                break;
        }

        return View("Index", model);
    }
    
    [HttpPost]
    public async Task<IActionResult> DeleteSavedReport(int id)
    {
        await _reportApiService.DeleteSavedReportAsync(id);
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    public async Task<IActionResult> RenameSavedReport(int id, string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            await _reportApiService.UpdateSavedReportAsync(id, new UpdateSavedReportViewModel
            {
                Name = name.Trim()
            });
        }

        return RedirectToAction(nameof(Index));
    }
}