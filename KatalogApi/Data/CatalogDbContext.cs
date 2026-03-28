using KatalogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace KatalogApi.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }
    
    public DbSet<Item> Items { get; set; }
    public DbSet<ErrorReport> ErrorReports { get; set; }
}