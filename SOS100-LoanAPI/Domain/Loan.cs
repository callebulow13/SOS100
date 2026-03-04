using System.ComponentModel.DataAnnotations;

namespace SOS100_LoansApi.Domain;

public class Loan
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public int ItemId { get; set; }

    [Required]
    [MaxLength(200)]
    public string BorrowerId { get; set; } = default!;

    public DateTimeOffset LoanedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset DueAt { get; set; }

    public DateTimeOffset? ReturnedAt { get; set; }

    public LoanStatus Status { get; set; } = LoanStatus.Active;
    
    public int? ActiveItemKey { get; set; }
    //Kommentera bort sålänge
    //[Timestamp] // vattentät concurrency
    //public byte[] RowVersion { get; set; } = default! ;
}