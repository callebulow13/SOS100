namespace SOS100_MVC.Dtos;

public class ReminderDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string LoanId { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public string ItemTitle { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public bool IsSent { get; set; }
}