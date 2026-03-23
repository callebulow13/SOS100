using ReportApi.DataProviders.Interfaces;
using ReportApi.DTOs.External;

namespace ReportApi.DataProviders;

public class LoanDataProvider : ILoanDataProvider
{
    private readonly HttpClient _httpClient;

    public LoanDataProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<LoanDto>> GetAllLoansAsync()
    {
        var response = await _httpClient.GetAsync("/api/loans");

        response.EnsureSuccessStatusCode();

        var loans = await response.Content.ReadFromJsonAsync<List<LoanDto>>();
        return loans ?? new List<LoanDto>();
    }
}