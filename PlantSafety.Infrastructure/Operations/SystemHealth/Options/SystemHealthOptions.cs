// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.SystemHealth
//  Layer:          Infrastructure
//  File:           SystemHealthOptions.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strongly typed configuration for the System Health rollup module.
//                  KnownTaskSchedulerJobs is the exact set of task names registered by
//                  the Phase 18 Express Checklist automation scripts:
//                    - Register-PlantSafetyBackupTasks.ps1 (3 tasks)
//                    - Register-PlantSafetyPurgeTasks.ps1 (5 tasks)
//                    - Register-PlantSafetyDatabaseSizeLogTask.ps1 (1 task)
//                  Additive-only evolution.
// ============================================================================
using System.Collections.Generic;

namespace PlantSafety.Infrastructure.Operations.SystemHealth.Options;

/// <summary>Strongly typed configuration for the System Health rollup module.</summary>
public sealed class SystemHealthOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "Operations:SystemHealth";

    /// <summary>
    /// The exact Windows Task Scheduler task names registered by the three Phase 18
    /// Register-PlantSafety*.ps1 scripts, paired with the category they belong to.
    /// Additive: new tasks may be appended if future scripts register more jobs.
    /// </summary>
    public IReadOnlyList<KnownTaskSchedulerJob> KnownTaskSchedulerJobs { get; init; } =
    [
        // Register-PlantSafetyBackupTasks.ps1
        new KnownTaskSchedulerJob("PlantSafety - Full Backup (All Databases)", "Backup"),
        new KnownTaskSchedulerJob("PlantSafety - Differential Backup (All Databases)", "Backup"),
        new KnownTaskSchedulerJob("PlantSafety - Transaction Log Backup (All Databases)", "Backup"),

        // Register-PlantSafetyPurgeTasks.ps1
        new KnownTaskSchedulerJob("PlantSafety - Purge AuditLog", "Purge"),
        new KnownTaskSchedulerJob("PlantSafety - Purge ErrorLog", "Purge"),
        new KnownTaskSchedulerJob("PlantSafety - Purge IntegrationLogEntry", "Purge"),
        new KnownTaskSchedulerJob("PlantSafety - Purge ReplayRecord", "Purge"),
        new KnownTaskSchedulerJob("PlantSafety - Purge DiagnosticEvent", "Purge"),

        // Register-PlantSafetyDatabaseSizeLogTask.ps1
        new KnownTaskSchedulerJob("PlantSafety - Database Size Snapshot", "DatabaseSizeLog"),
    ];

    /// <summary>Command timeout (seconds) applied to each schtasks.exe /Query invocation.</summary>
    public int SchTasksTimeoutSeconds { get; init; } = 15;
}

/// <summary>A single known Task Scheduler job name paired with its registering-script category.</summary>
/// <param name="TaskName">The exact registered task name.</param>
/// <param name="Category">"Backup" | "Purge" | "DatabaseSizeLog".</param>
public sealed record KnownTaskSchedulerJob(string TaskName, string Category);
