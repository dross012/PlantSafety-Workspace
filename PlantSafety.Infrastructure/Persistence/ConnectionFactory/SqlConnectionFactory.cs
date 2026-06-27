// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Persistence
//  Layer:          Infrastructure
//  File:           SqlConnectionFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default ISqlConnectionFactory backed by Microsoft.Data.SqlClient.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.ConnectionFactory;

/// <summary>
/// Default <see cref="ISqlConnectionFactory"/> backed by <see cref="SqlConnection"/>.
/// Opens connections eagerly; callers are responsible for disposal via <c>using</c>.
/// </summary>
public sealed class SqlConnectionFactory(IOptions<DatabaseOptions> options) : ISqlConnectionFactory
{
    private readonly DatabaseOptions _opts = options.Value;

    /// <inheritdoc/>
    public SqlConnection CreateConnection()
    {
        var conn = new SqlConnection(_opts.ConnectionString);
        conn.Open();
        return conn;
    }

    /// <inheritdoc/>
    public SqlConnection CreateReadConnection()
    {
        var connStr = string.IsNullOrWhiteSpace(_opts.ReadConnectionString)
            ? _opts.ConnectionString
            : _opts.ReadConnectionString;

        var conn = new SqlConnection(connStr);
        conn.Open();
        return conn;
    }
}
