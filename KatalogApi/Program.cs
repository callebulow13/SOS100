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

app.UseCors("AllowReactApp");

app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        await next(context);
        return;
    }

    // =========================================================================
    // NYTT: Om vi kör lokalt (Development), hoppa över alla säkerhetskontroller!
    // =========================================================================
    if (app.Environment.IsDevelopment())
    {
        await next(context); // Gå vidare direkt till controllern
        return; // Avbryt här så vi inte gör några fler API-kollar
    }
    // =========================================================================

    // --- NYTT: Släpp förbi webbläsaren till Scalar och OpenAPI utan nyckel ---
    var path = context.Request.Path;
    if (path.StartsWithSegments("/scalar") || path.StartsWithSegments("/openapi"))
    {
        await next(context); // Låt anropet gå vidare
        return; // Avbryt här så vi inte kollar nyckeln nedanför!
    }
    // -------------------------------------------------------------------------

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

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    // Kör alla väntande migrationer (skapar katalog.db om den inte finns!)
    context.Database.Migrate();
}

// --------------------------
app.Run();