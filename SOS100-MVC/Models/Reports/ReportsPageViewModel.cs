namespace SOS100_MVC.Models.Reports;

public class ReportsPageViewModel
{
    public string? SelectedReport { get; set; }
    public int? ItemId { get; set; }
    public string? ItemName { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public int? MostLoanedLimit { get; set; } = 20;
    public int? OverdueLoanCount { get; set; }
    public string? SavedReportName { get; set; }
    
    public List<SavedReportViewModel> SavedReports { get; set; } = new();
    public List<MostLoanedItemViewModel> MostLoanedItems { get; set; } = new();
    public List<ItemLoanHistoryViewModel> ItemLoanHistory { get; set; } = new();
    public List<UserLoanHistoryViewModel> UserLoanHistory { get; set; } = new();
    public List<CurrentLoanedItemViewModel> CurrentLoanedItems { get; set; } = new();
}