using KatalogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace KatalogApi.Data;

public class CatalogDbContext : DbContext
{
    // Konstruktor som tar emot inställningar (t.ex. vilken databas vi ska prata med)
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }
    
    // Här säger vi: "Skapa en SQL-tabell som heter Items, baserat på klassen Item"
    public DbSet<Item> Items { get; set; }
    public DbSet<LocationCoordinate> LocationCoordinates { get; set; }
}