namespace SOS100_MVC.Dtos;

public class LoanDto
{
    public Guid Id { get; set; }
    public int ItemId { get; set; }
    public string BorrowerId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    
    // Se till att dessa heter exakt som i ditt API!
    public DateTimeOffset LoanedAt { get; set; }
    public DateTimeOffset DueAt { get; set; }
    public DateTimeOffset? ReturnedAt { get; set; }
    
    public int Status { get; set; } 
}