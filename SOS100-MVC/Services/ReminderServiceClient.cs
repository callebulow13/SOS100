using SOS100_MVC.Models;

namespace SOS100_MVC.Services;

public class ReminderServiceClient
{
    private readonly HttpClient _http;

    public ReminderServiceClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Reminder>> GetRemindersAsync(int userId)
    {
        var response = await _http.GetAsync($"/api/reminders?userId={userId}");
        if (!response.IsSuccessStatusCode) return new List<Reminder>();
        return await response.Content.ReadFromJsonAsync<List<Reminder>>()
               ?? new List<Reminder>();
    }

    public async Task<int> GetOverdueCountAsync()
    {
        var response = await _http.GetAsync("/api/reminders/overdue/count");
        if (!response.IsSuccessStatusCode) return 0;
        var result = await response.Content
            .ReadFromJsonAsync<OverdueCountResult>();
        return result?.Count ?? 0;
    }

    public async Task<List<Watch>> GetWatchesAsync(int userId)
    {
        var response = await _http.GetAsync($"/api/watches?userId={userId}");
        if (!response.IsSuccessStatusCode) return new List<Watch>();
        return await response.Content.ReadFromJsonAsync<List<Watch>>()
               ?? new List<Watch>();
    }
}

public class OverdueCountResult
{
    public int Count { get; set; }
}