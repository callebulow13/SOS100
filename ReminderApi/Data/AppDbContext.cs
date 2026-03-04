using Microsoft.EntityFrameworkCore;
using ReminderApi.Models;

namespace ReminderApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Reminder> Reminders { get; set; }
    public DbSet<Watch> Watches { get; set; }
}