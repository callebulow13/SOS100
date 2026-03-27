namespace ReportApi.DTOs.Reports;

public class CreateSavedReportDto
{
    public string Name { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;

    public int? ItemId { get; set; }
    public string? ItemName { get; set; }

    public int? UserId { get; set; }
    public string? UserName { get; set; }

    public int? MostLoanedLimit { get; set; }
}