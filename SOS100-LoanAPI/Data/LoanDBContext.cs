using Microsoft.EntityFrameworkCore;
using SOS100_LoanAPI.Domain;

namespace SOS100_LoanAPI.Data;

public class LoanDbContext : DbContext
{
    public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options) { }

    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<LoanUserItemStat> LoanUserItemStats => Set<LoanUserItemStat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Regel i DB: endast ett "aktivt" lån per ItemId
        // Aktivt = ReturnedAt IS NULL
        modelBuilder.Entity<Loan>()
            .HasIndex(l => l.ItemId)
            .IsUnique()
            .HasFilter("\"ReturnedAt\" IS NULL");
        
        modelBuilder.Entity<LoanUserItemStat>()
            .HasIndex(s => new { s.BorrowerId, s.ItemId })
            .IsUnique();

        // Valfritt men bra för prestanda vid listning/filtrering
        modelBuilder.Entity<Loan>()
            .HasIndex(l => l.ReturnedAt);
    }
}