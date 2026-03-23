namespace SOS100_MVC.Models.Reports;

public class MostLoanedItemViewModel
{
    public int ItemId { get; set; }
    public string ItemTitle { get; set; } = string.Empty;
    public int LoanCount { get; set; }
}