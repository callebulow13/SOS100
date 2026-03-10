using ReportApi.Models;

namespace ReportApi.Data;

public static class SeedData
{
    public static void Initialize(ReportDbContext context)
    {
        if (context.Loans.Any())
        {
            return;
        }

        var loans = new List<LoanRecord>
        {
            new()
            {
                ItemId = 1,
                UserId = 1,
                LoanDate = DateTime.UtcNow.AddDays(-20),
                DueDate = DateTime.UtcNow.AddDays(-10),
                ReturnDate = DateTime.UtcNow.AddDays(-8)
            },
            new()
            {
                ItemId = 1,
                UserId = 2,
                LoanDate = DateTime.UtcNow.AddDays(-7),
                DueDate = DateTime.UtcNow.AddDays(-2),
                ReturnDate = null
            },
            new()
            {
                ItemId = 2,
                UserId = 1,
                LoanDate = DateTime.UtcNow.AddDays(-14),
                DueDate = DateTime.UtcNow.AddDays(-4),
                ReturnDate = null
            },
            new()
            {
                ItemId = 3,
                UserId = 3,
                LoanDate = DateTime.UtcNow.AddDays(-5),
                DueDate = DateTime.UtcNow.AddDays(5),
                ReturnDate = null
            },
            new()
            {
                ItemId = 1,
                UserId = 3,
                LoanDate = DateTime.UtcNow.AddDays(-30),
                DueDate = DateTime.UtcNow.AddDays(-20),
                ReturnDate = DateTime.UtcNow.AddDays(-18)
            }
        };

        context.Loans.AddRange(loans);
        context.SaveChanges();
    }
}