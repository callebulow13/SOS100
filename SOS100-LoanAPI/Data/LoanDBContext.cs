using Microsoft.EntityFrameworkCore;
using SOS100_LoansApi.Domain;

namespace SOS100_LoansApi.Data;

public class LoanDbContext : DbContext
{
    public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options) { }

    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Index för snabb uppslagning och för att stötta regeln "ett aktivt lån per Item"
        modelBuilder.Entity<Loan>()
            .HasIndex(l => new { l.ItemId, l.Status });
        
        // Endast ett aktivt lån per Item
        modelBuilder.Entity<Loan>()
            .HasIndex(l => l.ActiveItemKey)
            .IsUnique();
    }
}