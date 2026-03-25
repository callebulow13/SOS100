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
}