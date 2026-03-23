using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOS100_LoanAPI.Domain;

public class Loan
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public int ItemId { get; set; }

    [Required]
    [MaxLength(200)]
    public string BorrowerId { get; set; } = default!;
    
    [Required]
    [MaxLength(200)]
    public string ItemName { get; set; } = default!;

    public DateTimeOffset LoanedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset DueAt { get; set; }

    public DateTimeOffset? ReturnedAt { get; set; }

    // Status är nu HÄRLEDD från ReturnedAt och lagras inte i DB
    [NotMapped]
    public LoanStatus Status => ReturnedAt is null 
        ? LoanStatus.Active 
        : LoanStatus.Returned;
    
    
}