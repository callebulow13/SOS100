namespace ReportApi.Models;

public class LoanRecord
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public int UserId { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
}