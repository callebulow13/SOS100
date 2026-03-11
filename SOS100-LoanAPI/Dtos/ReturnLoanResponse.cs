namespace SOS100_LoanApi.Dtos;

public class ReturnLoanResponse
{
    public Guid LoanId { get; set; }
    public int ItemId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset ReturnedAt { get; set; }
}