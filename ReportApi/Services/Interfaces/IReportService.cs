using ReportApi.DTOs.Reports;

namespace ReportApi.Services.Interfaces;

public interface IReportService
{
    Task<List<MostLoanedItemReportDto>> GetMostLoanedItemsAsync(int? limit);
    Task<OverdueLoansReportDto> GetOverdueLoansAsync();
    Task<List<ItemLoanHistoryRowDto>> GetItemLoanHistoryAsync(int itemId);
    Task<List<UserLoanHistoryRowDto>> GetUserLoanHistoryAsync(int userId);
}