using KatalogApi.Data;
using KatalogApi.Models;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();

app.UseAuthorization();

app.Use(async (context, next) =>
{
    var path = context.Request.Path;

    if (path.StartsWithSegments("/scalar") || path.StartsWithSegments("/openapi"))
    {
        await next(context);
        return;
    }
    
    if (app.Environment.IsDevelopment())
    {
        await next(context);
        return;
    }

    var configuredApiKey = app.Configuration.GetValue<string>("KatalogApiKey");

    var providedApiKey = context.Request.Headers["X-Api-Key"].FirstOrDefault();

    if (string.IsNullOrEmpty(configuredApiKey) || providedApiKey != configuredApiKey)
    {
        context.Response.StatusCode = 401; // Unauthorized
        await context.Response.WriteAsync("Ogiltig eller saknad API-nyckel.");
        return;
    }

    await next(context);
});

app.UseCors("AllowReactApp");

app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    context.Database.Migrate();
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
        context.SaveChanges();
    }
}

app.Run();