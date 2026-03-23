namespace ReportApi.DTOs.External;

public class LoanDto
{
    public Guid Id { get; set; }

    public int ItemId { get; set; }

    public string BorrowerId { get; set; } = string.Empty;

    public DateTimeOffset LoanedAt { get; set; }

    public DateTimeOffset DueAt { get; set; }

    public DateTimeOffset? ReturnedAt { get; set; }
}