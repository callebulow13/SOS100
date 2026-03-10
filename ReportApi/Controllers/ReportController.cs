using Microsoft.AspNetCore.Mvc;
using ReportApi.Services;

namespace ReportApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("most-borrowed-items")]
    public async Task<IActionResult> GetMostBorrowedItems()
    {
        var result = await _reportService.GetMostBorrowedItemsAsync();
        return Ok(result);
    }

    [HttpGet("overdue-count")]
    public async Task<IActionResult> GetOverdueCount()
    {
        var result = await _reportService.GetOverdueLoansCountAsync();
        return Ok(result);
    }
}