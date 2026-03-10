using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using ReportApi.Data;
using ReportApi.Dtos;

namespace ReportApi.Services;

public class MixedReportDataProvider : IReportDataProvider
{
    private readonly ReportDbContext _context;
    private readonly HttpClient _httpClient;

    public MixedReportDataProvider(ReportDbContext context, HttpClient httpClient)
    {
        _context = context;
        _httpClient = httpClient;
    }

    public async Task<List<LoanDto>> GetLoansAsync()
    {
        return await _context.Loans
            .Select(l => new LoanDto
            {
                Id = l.Id,
                ItemId = l.ItemId,
                UserId = l.UserId,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnDate = l.ReturnDate
            })
            .ToListAsync();
    }

    public async Task<List<ItemDto>> GetItemsAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<ItemDto>>("api/items");
        return result ?? new List<ItemDto>();
    }

    public async Task<List<UserDto>> GetUsersAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<UserDto>>("api/users");
        return result ?? new List<UserDto>();
    }
}