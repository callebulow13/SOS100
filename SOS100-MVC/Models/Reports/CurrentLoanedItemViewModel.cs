namespace SOS100_MVC.Models.Reports;

public class CurrentLoanedItemViewModel
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTimeOffset LoanDate { get; set; }
}