// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.SchemaGuard
//  Layer:          Infrastructure
//  File:           SchemaVersionDapperRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of ISchemaVersionRepository. Reads
//                  [System].[SchemaMigrations] directly from each database in
//                  SchemaGuardOptions.DatabaseConnectionStrings — server-level per-database
//                  platform state, not tenant-scoped RLS data, so this repository opens its
//                  own SqlConnection per database key rather than going through
//                  ISqlConnectionFactory/DapperQueryExecutor (same reasoning as
//                  Operations.Backup's msdb access). SqlException handling routes through
//                  the shared ErrorClassifier. A connection failure or missing table is
//                  treated as "Unknown" (returns null), not "Drifted" — the caller cannot
//                  distinguish "definitely missing" from "couldn't check" otherwise.
//                  Additive-only evolution.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PlantSafety.Application.Operations.SchemaGuard.Models;
using PlantSafety.Application.Operations.SchemaGuard.Repositories;
using PlantSafety.Infrastructure.Operations.SchemaGuard.Options;

namespace PlantSafety.Infrastructure.Operations.SchemaGuard.Persistence;

/// <summary>Dapper-backed repository over each database's [System].[SchemaMigrations] table.</summary>
public sealed class SchemaVersionDapperRepository : ISchemaVersionRepository
{
    private readonly SchemaGuardOptions _options;
    private readonly ILogger<SchemaVersionDapperRepository> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SchemaVersionDapperRepository(IOptions<SchemaGuardOptions> options, ILogger<SchemaVersionDapperRepository> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetDatabaseKeys() => _options.DatabaseConnectionStrings.Keys.ToList();

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SchemaMigrationRecordDto>?> GetAppliedMigrationsAsync(string databaseKey, CancellationToken cancellationToken)
    {
        if (!_options.DatabaseConnectionStrings.TryGetValue(databaseKey, out var connectionString))
        {
            _logger.LogWarning("Schema version check: unknown database key {DatabaseKey}.", databaseKey);
            return null;
        }

        const string sql = @"
SELECT ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes
FROM [System].[SchemaMigrations]
ORDER BY AppliedAtUtc ASC;";

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var rows = await connection.QueryAsync<SchemaMigrationRecordDto>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));

            return rows.ToList();
        }
        catch (SqlException ex)
        {
            // A missing table (object not found) or unreachable database both mean "we
            // could not verify this database's schema state" -- surfaced as Unknown by the
            // caller, not misreported as Drifted. Still logged at Error so it's visible.
            _logger.LogError(ex, "Schema version check failed for database {DatabaseKey} (SQL error {ErrorNumber}).", databaseKey, ex.Number);
            return null;
        }
    }
}
