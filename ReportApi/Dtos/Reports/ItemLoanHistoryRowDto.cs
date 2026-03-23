namespace ReportApi.DTOs.Reports;

public class ItemLoanHistoryRowDto
{
    public Guid LoanId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public DateTimeOffset LoanDate { get; set; }

    public DateTimeOffset DueDate { get; set; }

    public DateTimeOffset? ReturnedDate { get; set; }
}