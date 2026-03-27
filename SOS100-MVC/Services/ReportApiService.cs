using System.Net.Http.Json;
using SOS100_MVC.Models.Reports;

namespace SOS100_MVC.Services;

public class ReportApiService
{
    private readonly HttpClient _httpClient;

    public ReportApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<MostLoanedItemViewModel>> GetMostLoanedItemsAsync(int? limit)
    {
        var url = "api/reports/most-loaned-items";

        if (limit.HasValue)
        {
            url += $"?limit={limit.Value}";
        }

        var result = await _httpClient.GetFromJsonAsync<List<MostLoanedItemViewModel>>(url);
        return result ?? new List<MostLoanedItemViewModel>();
    }

    public async Task<int?> GetOverdueLoansCountAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<OverdueLoansResponse>(
            "/api/reports/overdue-loans");

        return result?.OverdueLoanCount;
    }

    public async Task<List<ItemLoanHistoryViewModel>> GetItemLoanHistoryAsync(int itemId)
    {
        var result = await _httpClient.GetFromJsonAsync<List<ItemLoanHistoryViewModel>>(
            $"/api/reports/items/{itemId}/loan-history");

        return result ?? new List<ItemLoanHistoryViewModel>();
    }

    public async Task<List<ItemLoanHistoryViewModel>> GetItemLoanHistoryByNameAsync(string itemName)
    {
        var result = await _httpClient.GetFromJsonAsync<List<ItemLoanHistoryViewModel>>(
            $"/api/reports/items/loan-history/by-name?itemName={Uri.EscapeDataString(itemName)}");

        return result ?? new List<ItemLoanHistoryViewModel>();
    }

    public async Task<List<UserLoanHistoryViewModel>> GetUserLoanHistoryAsync(int userId)
    {
        var result = await _httpClient.GetFromJsonAsync<List<UserLoanHistoryViewModel>>(
            $"/api/reports/users/{userId}/loan-history");

        return result ?? new List<UserLoanHistoryViewModel>();
    }

    public async Task<List<UserLoanHistoryViewModel>> GetUserLoanHistoryByNameAsync(string userName)
    {
        var result = await _httpClient.GetFromJsonAsync<List<UserLoanHistoryViewModel>>(
            $"/api/reports/users/loan-history/by-name?userName={Uri.EscapeDataString(userName)}");

        return result ?? new List<UserLoanHistoryViewModel>();
    }
    
    public async Task<List<CurrentLoanedItemViewModel>> GetCurrentLoanedItemsAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<CurrentLoanedItemViewModel>>(
            "/api/reports/current-loaned-items");

        return result ?? new List<CurrentLoanedItemViewModel>();
    }
    
    public async Task<List<SavedReportViewModel>> GetSavedReportsAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<SavedReportViewModel>>(
            "/api/reports/saved-reports");

        return result ?? new List<SavedReportViewModel>();
    }

    public async Task<SavedReportViewModel?> GetSavedReportByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<SavedReportViewModel>(
            $"/api/reports/saved-reports/{id}");
    }

    public async Task<bool> CreateSavedReportAsync(CreateSavedReportViewModel model)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/reports/saved-reports", model);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateSavedReportAsync(int id, UpdateSavedReportViewModel model)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/reports/saved-reports/{id}", model);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteSavedReportAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"/api/reports/saved-reports/{id}");
        return response.IsSuccessStatusCode;
    }

    private class OverdueLoansResponse
    {
        public int OverdueLoanCount { get; set; }
    }
}