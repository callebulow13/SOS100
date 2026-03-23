namespace ReportApi.DTOs.Reports;

public class MostLoanedItemReportDto
{
    public int ItemId { get; set; }
    public string ItemTitle { get; set; } = string.Empty;
    public int LoanCount { get; set; }
}