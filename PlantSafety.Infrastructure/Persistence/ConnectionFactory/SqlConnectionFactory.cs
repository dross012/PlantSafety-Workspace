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
//
//                  RETRY POLICY (data-access-modernization pass, Checklist Section 2, item
//                  11.1): connection-open attempts are wrapped in a Polly retry pipeline
//                  (see SqlResiliencePipeline) that retries known-transient SqlException
//                  numbers with exponential back-off + jitter, mirroring the pattern already
//                  established by Intelligence.Infrastructure's SqlConnectionFactory. The
//                  public API stays synchronous (CreateConnection/CreateReadConnection) so
//                  no repository call site across the ~50 Dapper repositories that depend on
//                  ISqlConnectionFactory needed to change.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.ConnectionFactory;

/// <summary>
/// Default <see cref="ISqlConnectionFactory"/> backed by <see cref="SqlConnection"/>.
/// Opens connections eagerly (retrying transient SQL Server errors); callers are
/// responsible for disposal via <c>using</c>.
/// </summary>
public sealed class SqlConnectionFactory(IOptions<DatabaseOptions> options, ILogger<SqlConnectionFactory> logger) : ISqlConnectionFactory
{
    private readonly DatabaseOptions _opts = options.Value;

    /// <inheritdoc/>
    public SqlConnection CreateConnection()
    {
        var pipeline = SqlResiliencePipeline.CreateConnectionPipeline(logger, _opts.MaxRetryCount, nameof(CreateConnection));
        return pipeline.Execute(_ =>
        {
            var conn = new SqlConnection(_opts.ConnectionString);
            conn.Open();
            return conn;
        });
    }

    /// <inheritdoc/>
    public SqlConnection CreateReadConnection()
    {
        var connStr = string.IsNullOrWhiteSpace(_opts.ReadConnectionString)
            ? _opts.ConnectionString
            : _opts.ReadConnectionString;

        var pipeline = SqlResiliencePipeline.CreateConnectionPipeline(logger, _opts.MaxRetryCount, nameof(CreateReadConnection));
        return pipeline.Execute(_ =>
        {
            var conn = new SqlConnection(connStr);
            conn.Open();
            return conn;
        });
    }
}
