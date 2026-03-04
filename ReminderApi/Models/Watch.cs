namespace ReminderApi.Models;

public class Watch
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ItemId { get; set; }
    public string ItemTitle { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // En Watch kan ha många Reminders
    public List<Reminder> Reminders { get; set; } = new();
}