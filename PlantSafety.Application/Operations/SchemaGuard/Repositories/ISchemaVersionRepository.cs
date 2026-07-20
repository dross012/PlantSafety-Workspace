// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.SchemaGuard
//  Layer:          Application
//  File:           ISchemaVersionRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Reads the [System].[SchemaMigrations] table (Phase 7 Governance)
//                  that exists in each of the 8 live PlantSafety.* databases. Server-level
//                  per-database platform state, implemented in Infrastructure against each
//                  database's own connection string directly (not tenant-scoped RLS data,
//                  so ISqlConnectionFactory/DapperQueryExecutor do not apply here — same
//                  reasoning as Operations.Backup's msdb access). Additive-only evolution.
// ============================================================================
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PlantSafety.Application.Operations.SchemaGuard.Models;

namespace PlantSafety.Application.Operations.SchemaGuard.Repositories;

/// <summary>Repository over each database's [System].[SchemaMigrations] table.</summary>
public interface ISchemaVersionRepository
{
    /// <summary>The known database keys this repository can query (e.g. "CoreDB", "ArchiveDB"), from configuration.</summary>
    IReadOnlyList<string> GetDatabaseKeys();

    /// <summary>
    /// Returns every row from [System].[SchemaMigrations] for the given database key
    /// (e.g. "CoreDB"), or null if the database was unreachable or the table does not
    /// exist — callers treat null as "Unknown", not "Drifted".
    /// </summary>
    Task<IReadOnlyList<SchemaMigrationRecordDto>?> GetAppliedMigrationsAsync(string databaseKey, CancellationToken cancellationToken);
}
