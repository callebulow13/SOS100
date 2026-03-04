using System.ComponentModel.DataAnnotations;

namespace SOS100_LoanApi.Dtos;

public class CreateLoanRequest
{
    [Required]
    public int ItemId { get; set; }

    [Range(1, 60)]
    public int LoanDays { get; set; } = 14;

    // valfritt: om du senare vill stödja att admin kan skapa lån åt någon annan
    [MaxLength(200)]
    public string? BorrowerId { get; set; }
}