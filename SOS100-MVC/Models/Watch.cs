namespace SOS100_MVC.Models;

public class Watch
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ItemId { get; set; }
    public string? ItemTitle { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}