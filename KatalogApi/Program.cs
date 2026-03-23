using KatalogApi.Data;
using KatalogApi.Models;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Ändra till din Vite-port om den är annorlunda
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.Use(async (context, next) =>
{
    
    // Hoppar över API-nyckelkontroll i Development (lokal körning)
    
    if (app.Environment.IsDevelopment())
    {
        await next(context); // skickar vidare requesten
        return; // avslutar middleware här
    }

    // Tillåter Swagger / OpenAPI utan API-nyckel
    var path = context.Request.Path;
    if (path.StartsWithSegments("/scalar") || path.StartsWithSegments("/openapi"))
    {
        await next(context);
        return;
    }
    
    // Hämtar API-nyckel från konfiguration (appsettings / Azure)
    var configuredApiKey = app.Configuration.GetValue<string>("ApiKey");
    
    // Hämtar API-nyckel från request header (X-Api-Key)
    var providedApiKey = context.Request.Headers["X-Api-Key"].FirstOrDefault();
    
    // Validerar API-nyckel (matchning krävs)
    if (string.IsNullOrEmpty(configuredApiKey) || providedApiKey != configuredApiKey)
    {
        context.Response.StatusCode = 401; // Unauthorized
        await context.Response.WriteAsync("Ogiltig eller saknad API-nyckel.");
        return;
    }
    
    // Släpper igenom request om nyckeln är korrekt
    await next(context);
});

app.Use(async (context, next) =>
{
    // NYTT: Om vi kör lokalt (Development), hoppa över alla säkerhetskontroller!
    if (app.Environment.IsDevelopment())
    {
        await next(context); // Gå vidare direkt till controllern
        return; // Avbryt här så vi inte gör några fler API-kollar
    }

    // --- NYTT: Släpp förbi webbläsaren till Scalar och OpenAPI utan nyckel ---
    var path = context.Request.Path;
    if (path.StartsWithSegments("/scalar") || path.StartsWithSegments("/openapi"))
    {
        await next(context); // Låt anropet gå vidare
        return; // Avbryt här så vi inte kollar nyckeln nedanför!
    }

    // Litet bonustips: Använd app.Configuration istället för builder.Configuration här!
    var configuredApiKey = app.Configuration.GetValue<string>("KatalogApiKey");
    var extractedApiKey = context.Request.Headers["X-Api-Key"].FirstOrDefault();

    if (extractedApiKey != configuredApiKey)
    {
        context.Response.StatusCode = 401; 
        await context.Response.WriteAsync("Ogiltig eller saknad API-nyckel.");
        return; 
    }
    
    // Fixen är här nere:
    await next(context); 
});

app.UseCors("AllowReactApp");

app.MapControllers();
// --- SEEDING AV DATABAS ---
// Vi skapar ett tillfälligt "scope" för att få låna databas-bron under uppstarten
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    // Kör alla väntande migrationer (skapar katalog.db om den inte finns!)
    context.Database.Migrate();
    // Kollar om listan med prylar är helt tom
    if (!context.Items.Any())
    {
        // Om den är tom, lägg till dessa test-prylar!
        context.Items.AddRange(
            new Item 
            { 
                Name = "Bärbar Dator", 
                Type = ItemType.Elektronik, 
                Description = "MacBook Pro 2023, tillhör IT-avdelningen.", 
                Status = ItemStatus.Tillgänglig, 
                Placement = "IT-Skåpet", 
                PurchaseDate = DateTime.Now.AddYears(-1) 
            },
            new Item 
            { 
                Name = "Kaffebryggare", 
                Type = ItemType.Elektronik, 
                Description = "Moccamaster, brygger 10 koppar.", 
                Status = ItemStatus.Utlånad, 
                Placement = "Köket plan 2", 
                PurchaseDate = DateTime.Now.AddMonths(-6) 
            },
            new Item 
            { 
                Name = "C# för Nybörjare", 
                Type = ItemType.Bok, 
                Description = "Kurslitteratur för systemutvecklare.", 
                Status = ItemStatus.Tillgänglig, 
                Placement = "Bokhylla A", 
                PurchaseDate = DateTime.Now.AddDays(-14) 
            },
            new Item 
            { 
                Name = "Skruvdragare", 
                Type = ItemType.Annat, 
                Description = "Bosch 18V med dubbla batterier.", 
                Status = ItemStatus.Trasig, 
                Placement = "Verktygslådan", 
                PurchaseDate = DateTime.Now.AddDays(-25) 
            },
            new Item 
            { 
                Name = "Ekonomirapport", 
                Type = ItemType.Rapport, 
                Description = "Bokslut 2021-2022.", 
                Status = ItemStatus.Saknas, 
                Placement = "Bokhylla B", 
                PurchaseDate = DateTime.Now.AddDays(-25) 
            }
        );

        // Spara ändringarna till SQLite-filen
        context.SaveChanges();
    }
}
// --------------------------
app.Run();