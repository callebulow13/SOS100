using ReportApi.Dtos;

namespace ReportApi.Services;

public interface IReportService
{
    Task<List<MostBorrowedItemDto>> GetMostBorrowedItemsAsync();
    Task<int> GetOverdueLoansCountAsync();
}