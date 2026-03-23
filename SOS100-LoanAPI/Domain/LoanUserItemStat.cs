using System.ComponentModel.DataAnnotations;

namespace SOS100_LoanAPI.Domain;

public class LoanUserItemStat
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string BorrowerId { get; set; } = default!;

    [Required]
    public int ItemId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ItemName { get; set; } = default!;

    public int TotalLoans { get; set; } = 0;

    public int LateReturns { get; set; } = 0;
}