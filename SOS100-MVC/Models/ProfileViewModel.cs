namespace SOS100_MVC.Models;

// "Korgen" som vi skickar till vyn
public class ProfileViewModel
{
    public User User { get; set; } = new User();
    public List<LoanDto> ActiveLoans { get; set; } = new List<LoanDto>();
}

// Speglar kompisens Loan-klass i hans API
public class LoanDto
{
    public Guid Id { get; set; }
    public int ItemId { get; set; }
    public string BorrowerId { get; set; } = string.Empty;
    public DateTimeOffset LoanedAt { get; set; }
    public DateTimeOffset DueAt { get; set; }
    public DateTimeOffset? ReturnedAt { get; set; }
    public int Status { get; set; }
}