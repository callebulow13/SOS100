using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SOS100_MVC.Models;

namespace SOS100_MVC.Controllers;

public class LoanController : Controller
{
    private readonly HttpClient _catalogClient;
    private readonly HttpClient _loanClient;

    public LoanController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        // ---------- Katalog API ----------
        _catalogClient = new HttpClient();

        string katalogBaseUrl = configuration.GetValue<string>("KatalogApiBaseUrl");
        if (string.IsNullOrWhiteSpace(katalogBaseUrl))
            throw new ArgumentNullException("KatalogApiBaseUrl", "Hittar ingen webbadress till Katalog-API i appsettings.");

        _catalogClient.BaseAddress = new Uri(katalogBaseUrl);

        string katalogApiKey = configuration.GetValue<string>("KatalogApiKey");
        if (!string.IsNullOrWhiteSpace(katalogApiKey))
            _catalogClient.DefaultRequestHeaders.Add("X-Api-Key", katalogApiKey);

        // ---------- Loan API ----------
        _loanClient = httpClientFactory.CreateClient("LoanApi");
    }

    // GET: /Loan/Create?itemId=123
    [HttpGet]
    public async Task<IActionResult> Create(int itemId)
    {
        // 1) Hämta item från KatalogAPI så vyn kan visa informationen
        HttpResponseMessage response = await _catalogClient.GetAsync($"/api/items/{itemId}");

        if (response.IsSuccessStatusCode)
        {
            string data = await response.Content.ReadAsStringAsync();
            var item = JsonSerializer.Deserialize<Item>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (item == null)
                return NotFound("Kunde inte tolka item från KatalogAPI.");

            return View(item); // Views/Loan/Create.cshtml (Item som model)
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return NotFound("Kunde inte hitta item i katalogen.");

        return View("Error");
    }

    // POST: /Loan/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateLoanRequestVm form)
    {
        // TA BORT if-satsen helt och hållet!
        // Vi tvingar ALLTID BorrowerId att vara det unika siffer-ID:t (t.ex. "1")
        form.BorrowerId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? "";

        if (!ModelState.IsValid)
        {
            TempData["LoanError"] = "Formuläret innehåller fel. Kontrollera lånedagar och att BorrowerId finns.";
            return RedirectToAction(nameof(Create), new { itemId = form.ItemId });
        }

        // ... resten av din kod är exakt likadan

        // 2) Skicka låneförfrågan till LoanAPI
        HttpResponseMessage response = await _loanClient.PostAsJsonAsync("/api/loans", form);

        if (response.IsSuccessStatusCode)
        {
            TempData["LoanSuccess"] = "Lånet skapades! Prylen ska nu vara markerad som utlånad i katalogen.";
            return RedirectToAction("Details", "Catalog", new { id = form.ItemId });
        }

        // 3) Läs felmeddelande från LoanAPI (den skickar ofta { message = "..." })
        string errorText = await response.Content.ReadAsStringAsync();
        TempData["LoanError"] = ExtractApiMessage(errorText) ??
                                $"LoanAPI svarade med fel: {(int)response.StatusCode} {response.StatusCode}";

        return RedirectToAction(nameof(Create), new { itemId = form.ItemId });
    }

    private static string? ExtractApiMessage(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);

            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                string? msg = null;
                string? details = null;

                if (doc.RootElement.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String)
                    msg = m.GetString();

                if (doc.RootElement.TryGetProperty("detaljer", out var d) && d.ValueKind == JsonValueKind.String)
                    details = d.GetString();

                if (!string.IsNullOrWhiteSpace(msg) && !string.IsNullOrWhiteSpace(details))
                    return $"{msg} ({details})";

                if (!string.IsNullOrWhiteSpace(msg))
                    return msg;

                if (doc.RootElement.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                    return title.GetString();
            }
        }
        catch
        {
            // inte JSON, ignorera
        }

        if (!string.IsNullOrWhiteSpace(raw) && raw.Length < 500)
            return raw;

        return null;
    }
}