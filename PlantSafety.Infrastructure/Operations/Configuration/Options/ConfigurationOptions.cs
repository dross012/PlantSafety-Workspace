// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.Configuration
//  Layer:          Infrastructure
//  File:           ConfigurationOptions.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strongly typed options for the Configuration Framework (feature-flag
//                  editor). Bound from configuration section "Operations:Configuration".
//                  Only PlantSafety.ArchiveDB and PlantSafety.IntegrationDB are confirmed
//                  (by reading the full 144-feature-flags.sql migration, not just its
//                  first two USE blocks) to have a [System].[FeatureFlag] table — the
//                  other 6 live PlantSafety.* databases enumerated in SchemaGuardOptions
//                  do NOT. This repository therefore connects directly to each of these
//                  two databases by key rather than going through ISqlConnectionFactory
//                  (tenant-scoped primary/read-replica only) — same reasoning as
//                  Operations.SchemaGuard/Archival/Backup.
//                  Additive-only evolution.
// ============================================================================
using System.Collections.Generic;

namespace PlantSafety.Infrastructure.Operations.Configuration.Options;

/// <summary>Strongly typed configuration for the Configuration Framework (feature-flag editor).</summary>
public sealed class ConfigurationOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "Operations:Configuration";

    /// <summary>
    /// Logical database key -&gt; connection string, one entry per database confirmed (via
    /// 144-feature-flags.sql) to own a [System].[FeatureFlag] table. Defaults match the
    /// two live databases on the local SQLEXPRESS instance.
    /// </summary>
    public Dictionary<string, string> DatabaseConnectionStrings { get; init; } = new()
    {
        ["ArchiveDB"] = "Server=(local)\\SQLEXPRESS;Database=PlantSafety.ArchiveDB;Integrated Security=true;TrustServerCertificate=true;",
        ["IntegrationDB"] = "Server=(local)\\SQLEXPRESS;Database=PlantSafety.IntegrationDB;Integrated Security=true;TrustServerCertificate=true;",
    };
}
