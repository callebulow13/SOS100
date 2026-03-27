using Microsoft.AspNetCore.Mvc;
using ReportApi.DTOs.Reports;
using ReportApi.Services.Interfaces;

namespace ReportApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("most-loaned-items")]
    public async Task<ActionResult<List<MostLoanedItemReportDto>>> GetMostLoanedItems([FromQuery] int? limit)
    {
        var result = await _reportService.GetMostLoanedItemsAsync(limit);
        return Ok(result);
    }

    [HttpGet("overdue-loans")]
    public async Task<IActionResult> GetOverdueLoans()
    {
        var result = await _reportService.GetOverdueLoansAsync();
        return Ok(result);
    }

    [HttpGet("items/{itemId}/loan-history")]
    public async Task<IActionResult> GetItemLoanHistory(int itemId)
    {
        var result = await _reportService.GetItemLoanHistoryAsync(itemId);
        return Ok(result);
    }

    [HttpGet("items/loan-history/by-name")]
    public async Task<IActionResult> GetItemLoanHistoryByName([FromQuery] string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
            return BadRequest("Objektnamn måste anges.");

        var result = await _reportService.GetItemLoanHistoryByNameAsync(itemName);
        return Ok(result);
    }

    [HttpGet("users/{userId}/loan-history")]
    public async Task<IActionResult> GetUserLoanHistory(int userId)
    {
        var result = await _reportService.GetUserLoanHistoryAsync(userId);
        return Ok(result);
    }

    [HttpGet("users/loan-history/by-name")]
    public async Task<IActionResult> GetUserLoanHistoryByName([FromQuery] string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return BadRequest("Användarnamn måste anges.");

        var result = await _reportService.GetUserLoanHistoryByNameAsync(userName);
        return Ok(result);
    }
    
    [HttpGet("current-loaned-items")]
    public async Task<IActionResult> GetCurrentLoanedItems()
    {
        var result = await _reportService.GetCurrentLoanedItemsAsync();
        return Ok(result);
    }
    
    [HttpPost("saved-reports")]
    public async Task<IActionResult> CreateSavedReport([FromBody] CreateSavedReportDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Rapportnamn måste anges.");

        var result = await _reportService.CreateSavedReportAsync(dto);
        return CreatedAtAction(nameof(GetSavedReportById), new { id = result.Id }, result);
    }

    [HttpGet("saved-reports")]
    public async Task<IActionResult> GetSavedReports()
    {
        var result = await _reportService.GetSavedReportsAsync();
        return Ok(result);
    }

    [HttpGet("saved-reports/{id}")]
    public async Task<IActionResult> GetSavedReportById(int id)
    {
        var result = await _reportService.GetSavedReportByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPut("saved-reports/{id}")]
    public async Task<IActionResult> UpdateSavedReport(int id, [FromBody] UpdateSavedReportDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Rapportnamn måste anges.");

        var updated = await _reportService.UpdateSavedReportAsync(id, dto);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("saved-reports/{id}")]
    public async Task<IActionResult> DeleteSavedReport(int id)
    {
        var deleted = await _reportService.DeleteSavedReportAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}