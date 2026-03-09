using System.ComponentModel.DataAnnotations;

namespace SOS100_MVC.Models;

public class CreateLoanRequestVm
{
    [Required]
    public int ItemId { get; set; }

    [Range(1, 60)]
    public int LoanDays { get; set; } = 14;

    [Required]
    [MaxLength(200)]
    public string BorrowerId { get; set; } = string.Empty;
}