// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.Monitoring
//  Layer:          Infrastructure
//  File:           MonitoringOptions.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strongly typed options for the Operations.Monitoring module. Bound
//                  from configuration section "Operations:Monitoring".
//                    - DatabaseConnectionStrings: per-database Query Store access (DB
//                      Performance), same 8-database key set as SchemaGuardOptions.
//                    - ErrorDbConnectionString: [PlantSafety.ErrorDB] shared,
//                      cross-database platform error log (DB Errors).
//                    - ServerConnectionString: any database with VIEW SERVER STATE
//                      permission, used to read the server-scoped "system_health"
//                      Extended Events session ring buffer (DB Deadlocks).
//                  None of this is tenant-scoped, so none of it goes through
//                  ISqlConnectionFactory -- mirrors Operations.Backup/SchemaGuard.
//                  Additive-only evolution.
// ============================================================================
using System.Collections.Generic;

namespace PlantSafety.Infrastructure.Operations.Monitoring.Options;

/// <summary>Strongly typed configuration for the Operations.Monitoring module.</summary>
public sealed class MonitoringOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "Operations:Monitoring";

    /// <summary>
    /// Logical database key -&gt; connection string, one entry per live database with
    /// Query Store enabled (Phase 11, 111-query-store-configuration.sql). Defaults match
    /// the same 8 databases enumerated in SchemaGuardOptions on the local  PLANTSAFETY instance.
    /// </summary>
    public Dictionary<string, string> DatabaseConnectionStrings { get; init; } = new()
    {
        ["CoreDB"] = "Server=(local)\\PLANTSAFETY;Database=PlantSafety.CoreDB;Integrated Security=true;TrustServerCertificate=true;",
        ["ArchiveDB"] = "Server=(local)\\PLANTSAFETY;Database=PlantSafety.ArchiveDB;Integrated Security=true;TrustServerCertificate=true;",
        ["CloudIdentityDB"] = "Server=(local)\\PLANTSAFETY;Database=PlantSafety.CloudIdentityDB;Integrated Security=true;TrustServerCertificate=true;",
        ["PredictiveDB"] = "Server=(local)\\PLANTSAFETY;Database=PlantSafety.PredictiveDB;Integrated Security=true;TrustServerCertificate=true;",
        ["ReportingDB"] = "Server=(local)\\PLANTSAFETY;Database=PlantSafety.ReportingDB;Integrated Security=true;TrustServerCertificate=true;",
        ["IntegrationDB"] = "Server=(local)\\PLANTSAFETY;Database=PlantSafety.IntegrationDB;Integrated Security=true;TrustServerCertificate=true;",
        ["DiagnosticsDB"] = "Server=(local)\\PLANTSAFETY;Database=PlantSafety.DiagnosticsDB;Integrated Security=true;TrustServerCertificate=true;",
        ["ExportImportDB"] = "Server=(local)\\PLANTSAFETY;Database=PlantSafety.ExportImportDB;Integrated Security=true;TrustServerCertificate=true;",
    };

    /// <summary>
    /// Connection string targeting [PlantSafety.ErrorDB], the shared cross-database
    /// platform table written by every canonical stored procedure's CATCH block
    /// (see Phase8, 83-synonyms-audit-error.sql, and Phase17, 170-fix-errorlog-masking-bug.sql).
    /// </summary>
    public string ErrorDbConnectionString { get; init; } =
        "Server=(local)\\PLANTSAFETY;Database=PlantSafety.ErrorDB;Integrated Security=true;TrustServerCertificate=true;";

    /// <summary>
    /// Connection string used to read server-scoped DMVs (sys.dm_xe_sessions,
    /// sys.dm_xe_session_targets) for the built-in "system_health" Extended Events
    /// session. Any database reachable with VIEW SERVER STATE permission works; defaults
    /// to master.
    /// </summary>
    public string ServerConnectionString { get; init; } =
        "Server=(local)\\PLANTSAFETY;Database=master;Integrated Security=true;TrustServerCertificate=true;";

    /// <summary>
    /// Name of the Extended Events session whose ring buffer is read for deadlock events.
    /// Phase13-Monitoring\131-extended-events-sessions.sql defines only
    /// PlantSafety_ErrorCapture and PlantSafety_LongRunningQueries -- neither subscribes to
    /// sqlserver.xml_deadlock_report -- so this defaults to the built-in, always-on
    /// "system_health" session (the SQL Server Express default deadlock capture mechanism).
    /// </summary>
    public string DeadlockSessionName { get; init; } = "system_health";

    /// <summary>Default lookback window (hours) for the DB Performance / Query Store query.</summary>
    public int DefaultQueryPerformanceLookbackHours { get; init; } = 24;
}
