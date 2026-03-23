namespace SOS100_MVC.Models.Reports;

public class ItemLoanHistoryViewModel
{
    public Guid LoanId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTimeOffset LoanDate { get; set; }
    public DateTimeOffset DueDate { get; set; }
    public DateTimeOffset? ReturnedDate { get; set; }
}