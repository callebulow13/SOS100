using ReportApi.DataProviders.Interfaces;
using ReportApi.DTOs.Reports;
using ReportApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ReportApi.Data;
using ReportApi.Models;

namespace ReportApi.Services;

public class ReportService : IReportService
{
    private readonly ILoanDataProvider _loanDataProvider;
    private readonly IItemDataProvider _itemDataProvider;
    private readonly IUserDataProvider _userDataProvider;
    private readonly ReportDbContext _dbContext;

    public ReportService(
        ILoanDataProvider loanDataProvider,
        IItemDataProvider itemDataProvider,
        IUserDataProvider userDataProvider,
        ReportDbContext dbContext)
    {
        _loanDataProvider = loanDataProvider;
        _itemDataProvider = itemDataProvider;
        _userDataProvider = userDataProvider;
        _dbContext = dbContext;
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
                LoanCount = loanCounts.TryGetValue(item.Id, out var count) ? count : 0
            })
            .OrderByDescending(x => x.LoanCount)
            .ThenBy(x => x.ItemTitle)
            .ToList();

        if (limit.HasValue && limit.Value > 0)
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

    public async Task<List<ItemLoanHistoryRowDto>> GetItemLoanHistoryByNameAsync(string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
            return new List<ItemLoanHistoryRowDto>();

        var items = await _itemDataProvider.GetAllItemsAsync();

        var matchingItem = items.FirstOrDefault(i =>
            !string.IsNullOrWhiteSpace(i.Name) &&
            i.Name.Contains(itemName, StringComparison.OrdinalIgnoreCase));

        if (matchingItem == null)
            return new List<ItemLoanHistoryRowDto>();

        return await GetItemLoanHistoryAsync(matchingItem.Id);
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

    public async Task<List<UserLoanHistoryRowDto>> GetUserLoanHistoryByNameAsync(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return new List<UserLoanHistoryRowDto>();

        var users = await _userDataProvider.GetAllUsersAsync();

        var matchingUser = users.FirstOrDefault(u =>
            (!string.IsNullOrWhiteSpace(u.FullName) &&
             u.FullName.Contains(userName, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrWhiteSpace(u.Username) &&
             u.Username.Contains(userName, StringComparison.OrdinalIgnoreCase)));

        if (matchingUser == null)
            return new List<UserLoanHistoryRowDto>();

        return await GetUserLoanHistoryAsync(matchingUser.UserID);
    }
    
    public async Task<List<CurrentLoanedItemRowDto>> GetCurrentLoanedItemsAsync()
    {
        var loans = await _loanDataProvider.GetAllLoansAsync();
        var items = await _itemDataProvider.GetAllItemsAsync();
        var users = await _userDataProvider.GetAllUsersAsync();

        return loans
            .Where(l => l.ReturnedAt == null)
            .OrderByDescending(l => l.LoanedAt)
            .Select(l =>
            {
                var item = items.FirstOrDefault(i => i.Id == l.ItemId);

                var user = users.FirstOrDefault(u => u.UserID.ToString() == l.BorrowerId)
                           ?? users.FirstOrDefault(u => u.Username == l.BorrowerId);

                return new CurrentLoanedItemRowDto
                {
                    ItemId = l.ItemId,
                    ItemName = item?.Name ?? "Okänt objekt",
                    UserName = user?.FullName ?? user?.Username ?? "Okänd användare",
                    LoanDate = l.LoanedAt
                };
            })
            .ToList();
    }
    
    public async Task<SavedReportDto> CreateSavedReportAsync(CreateSavedReportDto dto)
{
    var savedReport = new SavedReport
    {
        Name = dto.Name,
        ReportType = dto.ReportType,
        ItemId = dto.ItemId,
        ItemName = dto.ItemName,
        UserId = dto.UserId,
        UserName = dto.UserName,
        MostLoanedLimit = dto.MostLoanedLimit,
        CreatedAt = DateTime.UtcNow
    };

    _dbContext.SavedReports.Add(savedReport);
    await _dbContext.SaveChangesAsync();

    return new SavedReportDto
    {
        Id = savedReport.Id,
        Name = savedReport.Name,
        ReportType = savedReport.ReportType,
        ItemId = savedReport.ItemId,
        ItemName = savedReport.ItemName,
        UserId = savedReport.UserId,
        UserName = savedReport.UserName,
        MostLoanedLimit = savedReport.MostLoanedLimit,
        CreatedAt = savedReport.CreatedAt
    };
}

public async Task<List<SavedReportDto>> GetSavedReportsAsync()
{
    return await _dbContext.SavedReports
        .OrderByDescending(x => x.CreatedAt)
        .Select(x => new SavedReportDto
        {
            Id = x.Id,
            Name = x.Name,
            ReportType = x.ReportType,
            ItemId = x.ItemId,
            ItemName = x.ItemName,
            UserId = x.UserId,
            UserName = x.UserName,
            MostLoanedLimit = x.MostLoanedLimit,
            CreatedAt = x.CreatedAt
        })
        .ToListAsync();
}

public async Task<SavedReportDto?> GetSavedReportByIdAsync(int id)
{
    var x = await _dbContext.SavedReports.FindAsync(id);

    if (x == null)
        return null;

    return new SavedReportDto
    {
        Id = x.Id,
        Name = x.Name,
        ReportType = x.ReportType,
        ItemId = x.ItemId,
        ItemName = x.ItemName,
        UserId = x.UserId,
        UserName = x.UserName,
        MostLoanedLimit = x.MostLoanedLimit,
        CreatedAt = x.CreatedAt
    };
}

public async Task<bool> UpdateSavedReportAsync(int id, UpdateSavedReportDto dto)
{
    var report = await _dbContext.SavedReports.FindAsync(id);

    if (report == null)
        return false;

    report.Name = dto.Name;
    await _dbContext.SaveChangesAsync();
    return true;
}

public async Task<bool> DeleteSavedReportAsync(int id)
{
    var report = await _dbContext.SavedReports.FindAsync(id);

    if (report == null)
        return false;

    _dbContext.SavedReports.Remove(report);
    await _dbContext.SaveChangesAsync();
    return true;
}
}