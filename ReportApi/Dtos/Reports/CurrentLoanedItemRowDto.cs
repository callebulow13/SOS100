namespace ReportApi.DTOs.Reports;

public class CurrentLoanedItemRowDto
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTimeOffset LoanDate { get; set; }
}