using System.Net;
using System.Net.Http.Json;
using ReportApi.DataProviders.Interfaces;
using ReportApi.DTOs.External;

namespace ReportApi.DataProviders;

public class ItemDataProvider : IItemDataProvider
{
    private readonly HttpClient _httpClient;

    public ItemDataProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ItemDto>> GetAllItemsAsync()
    {
        var response = await _httpClient.GetAsync("/api/items");
        response.EnsureSuccessStatusCode();

        var items = await response.Content.ReadFromJsonAsync<List<ItemDto>>();
        return items ?? new List<ItemDto>();
    }

    public async Task<ItemDto?> GetItemByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"/api/items/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ItemDto>();
    }
}