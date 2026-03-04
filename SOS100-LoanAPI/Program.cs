using Microsoft.EntityFrameworkCore;
using SOS100_LoansApi.Data;
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

// Läggs till i kompisens Program.cs (innan builder.Build())
builder.Services.AddHttpClient("KatalogClient", client =>
{
    // Kompisen måste skriva in porten som DITT API körs på (t.ex. 5017 eller 7032)
    client.BaseAddress = new Uri("http://localhost:5017"); 
    
    // Här skickar kompisen med nyckeln så att din dörrvakt släpper in anropet!
    client.DefaultRequestHeaders.Add("X-Api-Key", "HemligNyckel123"); 
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<LoanDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("LoansDb")));

var app = builder.Build();
app.UseExceptionHandler();

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
    var context = scope.ServiceProvider.GetRequiredService<SOS100_LoansApi.Data.LoanDbContext>();
    
    // Detta kommando tvingar databasen att skapas och bygga alla tabeller (Loans) 
    // utifrån era modeller, varje gång API:et startar!
    context.Database.EnsureCreated(); 
}
// --------------------------------------
app.Run();