using ReportApi.Dtos;

namespace ReportApi.Services;

public class ReportService : IReportService
{
    private readonly IReportDataProvider _dataProvider;

    public ReportService(IReportDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public async Task<List<MostBorrowedItemDto>> GetMostBorrowedItemsAsync()
    {
        var loans = await _dataProvider.GetLoansAsync();
        var items = await _dataProvider.GetItemsAsync();

        var result = loans
            .GroupBy(l => l.ItemId)
            .Select(group =>
            {
                var item = items.FirstOrDefault(i => i.Id == group.Key);

                return new MostBorrowedItemDto
                {
                    ItemId = group.Key,
                    Title = item?.Title ?? "Unknown item",
                    LoanCount = group.Count()
                };
            })
            .OrderByDescending(x => x.LoanCount)
            .ToList();

        return result;
    }

    public async Task<int> GetOverdueLoansCountAsync()
    {
        var loans = await _dataProvider.GetLoansAsync();

        return loans.Count(l =>
            l.ReturnDate == null &&
            l.DueDate < DateTime.UtcNow);
    }
}