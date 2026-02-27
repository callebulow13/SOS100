using KatalogApi.Data;
using KatalogApi.Models;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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
            }
        );

        // Spara ändringarna till SQLite-filen
        context.SaveChanges();
    }
}
// --------------------------
app.Run();