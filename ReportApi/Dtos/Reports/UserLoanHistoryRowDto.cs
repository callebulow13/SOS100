namespace ReportApi.DTOs.Reports;

public class UserLoanHistoryRowDto
{
    public Guid LoanId { get; set; }

    public string ItemTitle { get; set; } = string.Empty;

    public DateTimeOffset LoanDate { get; set; }

    public DateTimeOffset DueDate { get; set; }

    public DateTimeOffset? ReturnedDate { get; set; }
}