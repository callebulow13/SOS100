namespace ReportApi.Dtos;

public class MostBorrowedItemDto
{
    public int ItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int LoanCount { get; set; }
}