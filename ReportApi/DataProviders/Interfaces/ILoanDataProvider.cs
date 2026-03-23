using ReportApi.DTOs.External;

namespace ReportApi.DataProviders.Interfaces;

public interface ILoanDataProvider
{
    Task<List<LoanDto>> GetAllLoansAsync();
}