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
            case "most-loaned-items":
                model.MostLoanedItems = await _reportApiService
                    .GetMostLoanedItemsAsync(model.MostLoanedLimit);
                break;

            case "overdue":
                model.OverdueLoanCount = await _reportApiService.GetOverdueLoansCountAsync();
                break;

            case "item-history":
                if (model.ItemId.HasValue)
                {
                    model.ItemLoanHistory = await _reportApiService
                        .GetItemLoanHistoryAsync(model.ItemId.Value);
                }
                break;

            case "user-history":
                if (model.UserId.HasValue)
                {
                    model.UserLoanHistory = await _reportApiService
                        .GetUserLoanHistoryAsync(model.UserId.Value);
                }
                break;
        }

        return View(model);
    }
}