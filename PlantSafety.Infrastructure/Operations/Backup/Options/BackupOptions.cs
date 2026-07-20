// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.Backup
//  Layer:          Infrastructure
//  File:           BackupOptions.cs
//  Created:        2026-07-19
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strongly typed options for the SQL Server backup/DR automation
//                  surfaced by BackupDapperRepository. Bound from configuration
//                  section "Operations:Backup". All backup/DR data lives in msdb
//                  (server-level), NOT in a tenant database, so this repository
//                  deliberately does not use ISqlConnectionFactory (which targets the
//                  tenant-scoped primary/read-replica connection strings).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Operations.Backup.Options;

/// <summary>Strongly typed configuration for the SQL Server backup/DR automation.</summary>
public sealed class BackupOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "Operations:Backup";

    /// <summary>
    /// Connection string targeting the msdb system database, where
    /// usp_PlantSafety_Backup*, usp_PlantSafety_RestoreTest,
    /// PlantSafety_MaintenanceLog, and PlantSafety_DatabaseSizeLog all live.
    /// </summary>
    public string MsdbConnectionString { get; init; } =
        "Server=(local)\\SQLEXPRESS;Database=msdb;Integrated Security=true;TrustServerCertificate=true;";

    /// <summary>SQL Server instance name passed to Invoke-PlantSafetyBackup.ps1 -ServerInstance.</summary>
    public string ServerInstance { get; init; } = "(local)\\SQLEXPRESS";

    /// <summary>Absolute path to Invoke-PlantSafetyBackup.ps1 (Phase 18, Express Checklist).</summary>
    public string BackupScriptPath { get; init; } =
        @"C:\Users\dross\source\PlantSafety\PlantSafety.Infrastructure\Persistence\Migrations\Phase18-ExpressChecklist\Invoke-PlantSafetyBackup.ps1";

    /// <summary>
    /// Directory Invoke-PlantSafetyBackup.ps1 writes its timestamped Backup-&lt;Type&gt;-&lt;stamp&gt;.log
    /// files to. Surfaced to the UI as a reference path; not read directly by this repository.
    /// </summary>
    public string TaskLogDirectory { get; init; } = @"C:\Users\dross\SQLBackups\PlantSafety\TaskLogs";

    /// <summary>
    /// Absolute path/URL to the manual production-restore runbook. Surfaced by the UI
    /// instead of a live "Restore Production" button -- see DR-manual-restore-runbook.md.
    /// </summary>
    public string DrRunbookPath { get; init; } =
        @"C:\Users\dross\source\PlantSafety\PlantSafety.Infrastructure\Persistence\Migrations\Phase18-ExpressChecklist\DR-manual-restore-runbook.md";

    /// <summary>Command timeout (seconds) for the restore-verify call, which restores a full backup and runs DBCC CHECKDB.</summary>
    public int RestoreVerifyCommandTimeoutSeconds { get; init; } = 900;

    /// <summary>
    /// Hours since the last successful FULL backup after which a database's health is
    /// "Yellow" instead of "Green". Databases are backed up FULL weekly and DIFF daily
    /// per Phase 12, so 24h is a conservative freshness bar for the FULL backup signal
    /// specifically (differentials are not currently factored into this health signal).
    /// </summary>
    public int HealthWarnThresholdHours { get; init; } = 24;

    /// <summary>
    /// Hours since the last successful FULL backup after which a database's health is
    /// "Red" (or no FULL backup has ever completed). Chosen as double the warn threshold
    /// so a single missed weekly run does not immediately go critical, while two missed
    /// opportunities (or total absence of a FULL backup) does.
    /// </summary>
    public int HealthCriticalThresholdHours { get; init; } = 48;
}
