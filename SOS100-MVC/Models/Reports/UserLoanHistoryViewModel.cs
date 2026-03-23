namespace SOS100_MVC.Models.Reports;

public class UserLoanHistoryViewModel
{
    public Guid LoanId { get; set; }
    public string ItemTitle { get; set; } = string.Empty;
    public DateTimeOffset LoanDate { get; set; }
    public DateTimeOffset DueDate { get; set; }
    public DateTimeOffset? ReturnedDate { get; set; }
}