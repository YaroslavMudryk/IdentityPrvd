using System.Text.RegularExpressions;

namespace IdentityPrvd.Infrastructure.Database.Context.Configs;

public static class ConnectionStringHelper
{
    private static readonly Regex PostgresRegex = new Regex(
        @"host\s*=\s*[^;]+.*port\s*=\s*\d+.*username\s*=",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex SqlServerRegex = new Regex(
        @"data\s+source\s*=\s*[^;]+|initial\s+catalog\s*=",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex SqliteRegex = new Regex(
        @"data\s+source\s*=\s*[^;]*\.(sqlite|db)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static DatabaseType DetectDatabaseType(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return DatabaseType.Unknown;

        if (PostgresRegex.IsMatch(connectionString))
            return DatabaseType.Postgres;

        if (SqlServerRegex.IsMatch(connectionString))
            return DatabaseType.SqlServer;

        if (SqliteRegex.IsMatch(connectionString))
            return DatabaseType.Sqlite;

        return DatabaseType.Unknown;
    }
}
