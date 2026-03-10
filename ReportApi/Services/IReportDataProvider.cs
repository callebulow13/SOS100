using ReportApi.Dtos;

namespace ReportApi.Services;

public interface IReportDataProvider
{
    Task<List<LoanDto>> GetLoansAsync();
    Task<List<ItemDto>> GetItemsAsync();
    Task<List<UserDto>> GetUsersAsync();
}