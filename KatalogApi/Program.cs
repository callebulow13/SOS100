using KatalogApi.Data;
using KatalogApi.Models;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
// Konfigurerar CORS (Cross-Origin Resource Sharing).
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

// Konfigurerar Entity Framework Core att använda SQLite.
// Connection-strängen hämtas dynamiskt från appsettings.json.
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddOpenApi();

var app = builder.Build();

// Exponerar API-dokumentationen via OpenAPI och Scalar.
app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthorization();

// Middleware för API-nyckel
app.Use(async (context, next) =>
{
    var path = context.Request.Path;

    // Undantag 1: Tillåt alltid åtkomst till dokumentationen, även utan nyckel.
    if (path.StartsWithSegments("/scalar") || path.StartsWithSegments("/openapi"))
    {
        await next(context);
        return;
    }

    // Undantag 2: Om vi kör lokalt (Development) kan vi stänga av kravet
    if (app.Environment.IsDevelopment())
    {
        await next(context);
        return;
    }
    
    if (context.Request.Method == HttpMethods.Get)
    {
        await next(context);
        return;
    }

    // Hämtar den förväntade API-nyckeln från inställningarna
    var configuredApiKey = app.Configuration.GetValue<string>("KatalogApiKey");

    // Läs av den API-nyckel som klienten skickade med i sin HTTP-Header.
    var providedApiKey = context.Request.Headers["X-Api-Key"].FirstOrDefault();

    // Validering: Om nyckeln saknas i inställningarna eller om klientens nyckel är felaktig, stoppa anropet.
    if (string.IsNullOrEmpty(configuredApiKey) || providedApiKey != configuredApiKey)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Ogiltig eller saknad API-nyckel.");
        return;
    }

    // Om vi når hit var nyckeln korrekt. Skicka anropet vidare till nästa steg.
    await next(context);
});



app.MapControllers();

// Databasinitiering och seeding
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

    // Utför eventuella väntande migreringar och skapar databasfilen om den inte existerar.
    context.Database.Migrate();

    // Om databasen är helt tom, fyller vi den med grunddata (seeding) för biblioteket.
    if (!context.Items.Any())
    {
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
        // Sparar alla nya objekt till databasen.
        context.SaveChanges();
    }
}

app.Run();