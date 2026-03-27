using Microsoft.EntityFrameworkCore;
using ReportApi.Models;

namespace ReportApi.Data;

public class ReportDbContext : DbContext
{
    public ReportDbContext(DbContextOptions<ReportDbContext> options) : base(options)
    {
    }

    public DbSet<LoanRecord> Loans => Set<LoanRecord>();
    public DbSet<SavedReport> SavedReports => Set<SavedReport>();
}