using System.ComponentModel.DataAnnotations;

namespace SOS100_LoansApi.Domain;

public class Loan
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string ItemId { get; set; } = default!;

    [Required]
    [MaxLength(200)]
    public string BorrowerId { get; set; } = default!;

    public DateTimeOffset LoanedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset DueAt { get; set; }

    public DateTimeOffset? ReturnedAt { get; set; }

    public LoanStatus Status { get; set; } = LoanStatus.Active;

    [Timestamp] // vattentät concurrency
    public byte[] RowVersion { get; set; } = default!;
}