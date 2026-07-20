// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.SchemaGuard
//  Layer:          Infrastructure
//  File:           SchemaGuardOptions.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strongly typed options for the Schema Drift Guard. Bound from
//                  configuration section "Operations:SchemaGuard". Each of the 8 live
//                  PlantSafety.* databases (Phase 7 Governance, 70-schema-migrations-
//                  tracking.sql) tracks its own [System].[SchemaMigrations] table, so this
//                  repository connects to each database directly by key rather than going
//                  through ISqlConnectionFactory (tenant-scoped primary/read-replica only).
//                  Additive-only evolution.
// ============================================================================
using System.Collections.Generic;

namespace PlantSafety.Infrastructure.Operations.SchemaGuard.Options;

/// <summary>Strongly typed configuration for the Schema Drift Guard.</summary>
public sealed class SchemaGuardOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "Operations:SchemaGuard";

    /// <summary>
    /// Logical database key -&gt; connection string, one entry per live database tracked
    /// by Phase 7's [System].[SchemaMigrations] rollout. Defaults match the 8 databases
    /// enumerated in 70-schema-migrations-tracking.sql on the local SQLEXPRESS instance.
    /// </summary>
    public Dictionary<string, string> DatabaseConnectionStrings { get; init; } = new()
    {
        ["CoreDB"] = "Server=(local)\\SQLEXPRESS;Database=PlantSafety.CoreDB;Integrated Security=true;TrustServerCertificate=true;",
        ["ArchiveDB"] = "Server=(local)\\SQLEXPRESS;Database=PlantSafety.ArchiveDB;Integrated Security=true;TrustServerCertificate=true;",
        ["CloudIdentityDB"] = "Server=(local)\\SQLEXPRESS;Database=PlantSafety.CloudIdentityDB;Integrated Security=true;TrustServerCertificate=true;",
        ["PredictiveDB"] = "Server=(local)\\SQLEXPRESS;Database=PlantSafety.PredictiveDB;Integrated Security=true;TrustServerCertificate=true;",
        ["ReportingDB"] = "Server=(local)\\SQLEXPRESS;Database=PlantSafety.ReportingDB;Integrated Security=true;TrustServerCertificate=true;",
        ["IntegrationDB"] = "Server=(local)\\SQLEXPRESS;Database=PlantSafety.IntegrationDB;Integrated Security=true;TrustServerCertificate=true;",
        ["DiagnosticsDB"] = "Server=(local)\\SQLEXPRESS;Database=PlantSafety.DiagnosticsDB;Integrated Security=true;TrustServerCertificate=true;",
        ["ExportImportDB"] = "Server=(local)\\SQLEXPRESS;Database=PlantSafety.ExportImportDB;Integrated Security=true;TrustServerCertificate=true;",
    };

    /// <summary>Root folder scanned for migration script manifests. Defaults to Persistence\Migrations under Infrastructure.</summary>
    public string MigrationsRootPath { get; init; } =
        @"C:\Users\dross\source\PlantSafety\PlantSafety.Infrastructure\Persistence\Migrations";
}
