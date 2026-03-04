namespace ReminderApi.Models;

public class Reminder
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int LoanId { get; set; }
    public int ItemId { get; set; }
    public string ItemTitle { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public bool IsSent { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? WatchId { get; set; }
    public Watch? Watch { get; set; }
 
}