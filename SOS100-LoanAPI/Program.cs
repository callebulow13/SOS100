using Microsoft.EntityFrameworkCore;
using SOS100_LoanAPI.Data;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 30; // 30 requests
        limiterOptions.Window = TimeSpan.FromSeconds(10); // per 10 sekunder
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0; // inga köade requests, bara stoppa
    });
});

builder.Services.AddProblemDetails();

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// Läggs till i kompisens Program.cs (innan builder.Build())
builder.Services.AddHttpClient("KatalogClient", client =>
{
    var baseUrl = builder.Configuration["KatalogApiBaseUrl"];
    var apiKey = builder.Configuration["KatalogApiKey"];

    client.BaseAddress = new Uri(baseUrl!);
    client.DefaultRequestHeaders.Add("X-Api-Key", apiKey!);
});
builder.Services.AddHttpClient("ReminderApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ReminderApiBaseUrl"]!);

    client.DefaultRequestHeaders.Add("X-Api-Key",
        builder.Configuration["ReminderApiApiKey"]!);
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<LoanDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("LoansDb")));

var app = builder.Build();
app.UseExceptionHandler();

//app.UseSwagger();
//app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers().RequireRateLimiting("fixed");
// --- AUTOMATISK DATABAS-UPPDATERING ---

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SOS100_LoanAPI.Data.LoanDbContext>();

    // Kör migrations (uppdaterar schema och index på en befintlig DB)
    context.Database.Migrate();
}
// --------------------------------------
app.Run();