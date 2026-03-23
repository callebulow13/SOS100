using System.Net;
using System.Net.Http.Json;
using ReportApi.DataProviders.Interfaces;
using ReportApi.DTOs.External;

namespace ReportApi.DataProviders;

public class UserDataProvider : IUserDataProvider
{
    private readonly HttpClient _httpClient;

    public UserDataProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var response = await _httpClient.GetAsync("/User");

        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();

        return users ?? new List<UserDto>();
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"/User/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UserDto>();
    }
}