namespace SOS100_MVC.Models.Reports;

public class ReportsPageViewModel
{
    public string SelectedReport { get; set; } = string.Empty;

    public int? ItemId { get; set; }
    public int? UserId { get; set; }

    public int? MostLoanedLimit { get; set; } = 20;

    public int? OverdueLoanCount { get; set; }

    public List<MostLoanedItemViewModel> MostLoanedItems { get; set; } = new();
    public List<ItemLoanHistoryViewModel> ItemLoanHistory { get; set; } = new();
    public List<UserLoanHistoryViewModel> UserLoanHistory { get; set; } = new();
}