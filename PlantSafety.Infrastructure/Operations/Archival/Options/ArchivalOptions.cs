// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.Archival
//  Layer:          Infrastructure
//  File:           ArchivalOptions.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strongly typed options for the retention/purge automation surfaced by
//                  ArchivalDapperRepository. Bound from configuration section
//                  "Operations:Archival". Mirrors SchemaGuardOptions' per-database
//                  connection-string dictionary pattern: the 5 purge procedures each live
//                  in a different PlantSafety.* database, and Archival.ArchivalJob /
//                  Archival.TierPolicy both live in PlantSafety.ArchiveDB, so this
//                  repository connects to each database directly by key rather than going
//                  through ISqlConnectionFactory (tenant-scoped primary/read-replica only).
//                  Additive-only evolution.
// ============================================================================
using System.Collections.Generic;

namespace PlantSafety.Infrastructure.Operations.Archival.Options;

/// <summary>Strongly typed configuration for the retention/purge automation.</summary>
public sealed class ArchivalOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "Operations:Archival";

    /// <summary>
    /// Logical database key -&gt; connection string, one entry per database that hosts a
    /// purge stored procedure or the Archival.ArchivalJob/Archival.TierPolicy tables
    /// themselves (Phase7-Governance/74-retention-archival.sql).
    /// </summary>
    public Dictionary<string, string> DatabaseConnectionStrings { get; init; } = new()
    {
        ["AuditDB"] = "Server=(local)\\PLANTSAFETY;Database=PlantSafety.AuditDB;Integrated Security=true;TrustServerCertificate=true;",
        ["ErrorDB"] = "Server=(local)\\PLANTSAFETY;Database=PlantSafety.ErrorDB;Integrated Security=true;TrustServerCertificate=true;",
        ["IntegrationDB"] = "Server=(local)\\PLANTSAFETY;Database=PlantSafety.IntegrationDB;Integrated Security=true;TrustServerCertificate=true;",
        ["DiagnosticsDB"] = "Server=(local)\\PLANTSAFETY;Database=PlantSafety.DiagnosticsDB;Integrated Security=true;TrustServerCertificate=true;",
        ["ArchiveDB"] = "Server=(local)\\PLANTSAFETY;Database=PlantSafety.ArchiveDB;Integrated Security=true;TrustServerCertificate=true;",
    };

    /// <summary>
    /// Absolute path to Invoke-PlantSafetyPurge.ps1 (Phase 18, Express Checklist). Not
    /// invoked by ArchivalDapperRepository directly -- surfaced to the UI/operators as a
    /// reference to the SAME script Windows Task Scheduler already runs daily at 02:00, so
    /// the on-demand EXEC path documented on <see cref="Persistence.ArchivalDapperRepository"/>
    /// can be understood as "the same operation, run early" rather than a second code path.
    /// </summary>
    public string PurgeScriptPath { get; init; } =
        @"C:\Users\dross\source\PlantSafety\PlantSafety.Infrastructure\Persistence\Migrations\Phase18-ExpressChecklist\Invoke-PlantSafetyPurge.ps1";

    /// <summary>SQL Server instance name, surfaced alongside <see cref="PurgeScriptPath"/> for reference.</summary>
    public string ServerInstance { get; init; } = "(local)\\PLANTSAFETY";

    /// <summary>
    /// Directory Invoke-PlantSafetyPurge.ps1 writes its timestamped Purge-&lt;ProcName&gt;-&lt;stamp&gt;.log
    /// files to. Surfaced to the UI as a reference path for the scheduled (non-on-demand) runs.
    /// </summary>
    public string TaskLogDirectory { get; init; } = @"C:\Users\dross\SQLBackups\PlantSafety\TaskLogs";

    /// <summary>Command timeout (seconds) for each on-demand purge EXEC. These are fast, bounded DELETEs, so a short default is used.</summary>
    public int PurgeCommandTimeoutSeconds { get; init; } = 120;
}
