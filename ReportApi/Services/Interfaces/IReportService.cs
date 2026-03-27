using ReportApi.DTOs.Reports;

namespace ReportApi.Services.Interfaces;

public interface IReportService
{
    Task<List<MostLoanedItemReportDto>> GetMostLoanedItemsAsync(int? limit);
    Task<OverdueLoansReportDto> GetOverdueLoansAsync();
    Task<List<ItemLoanHistoryRowDto>> GetItemLoanHistoryAsync(int itemId);
    Task<List<ItemLoanHistoryRowDto>> GetItemLoanHistoryByNameAsync(string itemName);
    Task<List<UserLoanHistoryRowDto>> GetUserLoanHistoryAsync(int userId);
    Task<List<UserLoanHistoryRowDto>> GetUserLoanHistoryByNameAsync(string userName);
    Task<List<CurrentLoanedItemRowDto>> GetCurrentLoanedItemsAsync();
    Task<SavedReportDto> CreateSavedReportAsync(CreateSavedReportDto dto);
    Task<List<SavedReportDto>> GetSavedReportsAsync();
    Task<SavedReportDto?> GetSavedReportByIdAsync(int id);
    Task<bool> UpdateSavedReportAsync(int id, UpdateSavedReportDto dto);
    Task<bool> DeleteSavedReportAsync(int id);
}