using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace SOS100_LoanAPI.Infrastructure;

public static class DbUpdateExceptionExtensions
{
    public static bool IsSqliteUniqueConstraintViolation(this DbUpdateException ex)
    {
        // SQLite: constraint violations ger SqliteException med errorcode 19.
        // Extended codes kan vara 2067 (unique) eller 1555 (primary key).
        if (ex.InnerException is not SqliteException sqliteEx)
            return false;

        return sqliteEx.SqliteErrorCode == 19; // SQLITE_CONSTRAINT
    }
}