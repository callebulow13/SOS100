namespace SOS100_MVC.Models;

public class Reminder
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int LoanId { get; set; }
    public int ItemId { get; set; }
    public string? ItemTitle { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsSent { get; set; }
    public DateTime CreatedAt { get; set; }
}