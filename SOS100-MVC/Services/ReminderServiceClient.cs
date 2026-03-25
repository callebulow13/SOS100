using SOS100_MVC.Models;

namespace SOS100_MVC.Services;

public class ReminderServiceClient
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    // lägger till IConfiguration i konstruktorn för att kunna läsa appsettings
    public ReminderServiceClient(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;

        // Sätt BaseUrl ( sätter default till localhost:5038 om den saknas i appsettings)
        if (_http.BaseAddress == null)
        {
            var baseUrl = _config["ReminderServiceBaseUrl"] ?? "http://localhost:5038";
            _http.BaseAddress = new Uri(baseUrl);
        }

        // HÄR ÄR FIXEN: Hämta API-nyckeln och lägg till den i alla anrop!
        var apiKey = _config["ReminderApiKey"] ?? "reminder-hemlig-123"; 
        if (!_http.DefaultRequestHeaders.Contains("X-Api-Key"))
        {
            _http.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        }
    }

    // OBS! Kolla att detta är string om vi ändrade till string i tidigare steg!
    public async Task<List<Reminder>> GetRemindersAsync(string userId)
    {
        var response = await _http.GetAsync($"/api/reminders?userId={userId}");
    
        // Om anropet misslyckas, fånga det riktiga felet och kasta ett undantag!
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Stopp och belägg! API svarade {(int)response.StatusCode}. Felmeddelande: {error}. Kollar mot adress: {_http.BaseAddress}");
        }
    
        return await response.Content.ReadFromJsonAsync<List<Reminder>>() ?? new List<Reminder>();
    }

    public async Task<int> GetOverdueCountAsync()
    {
        var response = await _http.GetAsync("/api/reminders/overdue/count");
        if (!response.IsSuccessStatusCode) return 0;
        
        var result = await response.Content.ReadFromJsonAsync<OverdueCountResult>();
        return result?.Count ?? 0;
    }

    // OBS! Kolla att detta är string om vi ändrade till string i tidigare steg!
    public async Task<List<Watch>> GetWatchesAsync(string userId)
    {
        var response = await _http.GetAsync($"/api/watches?userId={userId}");
        if (!response.IsSuccessStatusCode) return new List<Watch>();
        
        return await response.Content.ReadFromJsonAsync<List<Watch>>()
               ?? new List<Watch>();
    }
    
    public async Task DeleteWatchAsync(int watchId)
    {
        var response = await _http.DeleteAsync($"/api/watches/{watchId}");
    
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Kunde inte ta bort bevakningen. API svarade med status {(int)response.StatusCode}: {error}");
        }
    }
}

public class OverdueCountResult
{
    public int Count { get; set; }
}