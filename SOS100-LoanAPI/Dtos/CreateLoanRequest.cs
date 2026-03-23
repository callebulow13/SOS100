using System.ComponentModel.DataAnnotations;

namespace SOS100_LoanApi.Dtos;

public class CreateLoanRequest
{
    [Required]
    public int ItemId { get; set; }

    [Range(1, 60)]
    public int LoanDays { get; set; } = 14;
    
    [MaxLength(200)]
    public string? BorrowerId { get; set; }
}