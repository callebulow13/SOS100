using ReportApi.DataProviders.Interfaces;
using ReportApi.DTOs.Reports;
using ReportApi.Services.Interfaces;

namespace ReportApi.Services;

public class ReportService : IReportService
{
    private readonly ILoanDataProvider _loanDataProvider;
    private readonly IItemDataProvider _itemDataProvider;
    private readonly IUserDataProvider _userDataProvider;

    public ReportService(
        ILoanDataProvider loanDataProvider,
        IItemDataProvider itemDataProvider,
        IUserDataProvider userDataProvider)
    {
        _loanDataProvider = loanDataProvider;
        _itemDataProvider = itemDataProvider;
        _userDataProvider = userDataProvider;
    }

    public async Task<List<MostLoanedItemReportDto>> GetMostLoanedItemsAsync(int? limit)
    {
        var loans = await _loanDataProvider.GetAllLoansAsync();
        var items = await _itemDataProvider.GetAllItemsAsync();

        var loanCounts = loans
            .GroupBy(l => l.ItemId)
            .ToDictionary(g => g.Key, g => g.Count());

        var report = items
            .Select(item => new MostLoanedItemReportDto
            {
                ItemId = item.Id,
                ItemTitle = item.Name,
                LoanCount = loanCounts.ContainsKey(item.Id) ? loanCounts[item.Id] : 0
            })
            .OrderByDescending(x => x.LoanCount)
            .ThenBy(x => x.ItemTitle)
            .ToList();

        if (limit.HasValue)
        {
            report = report.Take(limit.Value).ToList();
        }

        return report;
    }

    public async Task<OverdueLoansReportDto> GetOverdueLoansAsync()
    {
        var loans = await _loanDataProvider.GetAllLoansAsync();

        var overdueCount = loans.Count(l =>
            l.ReturnedAt == null && 
            l.DueAt < DateTimeOffset.UtcNow);

        return new OverdueLoansReportDto
        {
            OverdueLoanCount = overdueCount
        };
    }

    public async Task<List<ItemLoanHistoryRowDto>> GetItemLoanHistoryAsync(int itemId)
    {
        var loans = await _loanDataProvider.GetAllLoansAsync();
        var users = await _userDataProvider.GetAllUsersAsync();

        return loans
            .Where(l => l.ItemId == itemId)
            .OrderByDescending(l => l.LoanedAt)
            .Select(l =>
            {
                var user = users.FirstOrDefault(u => u.UserID.ToString() == l.BorrowerId)
                           ?? users.FirstOrDefault(u => u.Username == l.BorrowerId);

                return new ItemLoanHistoryRowDto
                {
                    LoanId = l.Id,
                    UserName = user?.FullName ?? "Okänd användare",
                    LoanDate = l.LoanedAt,
                    DueDate = l.DueAt,
                    ReturnedDate = l.ReturnedAt
                };
            })
            .ToList();
    }
    
    public async Task<List<UserLoanHistoryRowDto>> GetUserLoanHistoryAsync(int userId)
    {
        var loans = await _loanDataProvider.GetAllLoansAsync();
        var items = await _itemDataProvider.GetAllItemsAsync();

        return loans
            .Where(l => l.BorrowerId == userId.ToString())
            .OrderByDescending(l => l.LoanedAt)
            .Select(l =>
            {
                var item = items.FirstOrDefault(i => i.Id == l.ItemId);

                return new UserLoanHistoryRowDto
                {
                    LoanId = l.Id,
                    ItemTitle = item?.Name ?? "Okänd titel",
                    LoanDate = l.LoanedAt,
                    DueDate = l.DueAt,
                    ReturnedDate = l.ReturnedAt
                };
            })
            .ToList();
    }
}