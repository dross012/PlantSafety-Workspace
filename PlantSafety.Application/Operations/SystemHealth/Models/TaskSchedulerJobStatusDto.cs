// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.SystemHealth
//  Layer:          Application
//  File:           TaskSchedulerJobStatusDto.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Live status of a single Windows Task Scheduler task registered by the
//                  Phase 18 Express Checklist automation scripts (Register-PlantSafety
//                  BackupTasks.ps1 / Register-PlantSafetyPurgeTasks.ps1 / Register-
//                  PlantSafetyDatabaseSizeLogTask.ps1). Sourced via schtasks.exe rather
//                  than a database query -- see ISystemHealthRepository.
//                  Additive-only evolution.
// ============================================================================
using System;

namespace PlantSafety.Application.Operations.SystemHealth.Models;

/// <summary>Live status of a single registered Windows Task Scheduler automation task.</summary>
/// <param name="TaskName">The exact registered task name (e.g. "PlantSafety - Full Backup (All Databases)").</param>
/// <param name="Category">Which Phase 18 script registers this task: "Backup", "Purge", or "DatabaseSizeLog".</param>
/// <param name="Status">"Ready" | "Running" | "Disabled" | "Unknown" | "Unavailable" -- "Unavailable" means schtasks.exe could not be queried (not registered, not on Windows, parse failure, etc.), not a hard failure.</param>
/// <param name="LastRunTimeUtc">The task's last run time, if parsed successfully.</param>
/// <param name="LastResult">The task's last result code as reported by schtasks.exe, if parsed successfully ("0" means success).</param>
public sealed record TaskSchedulerJobStatusDto(
    string TaskName,
    string Category,
    string Status,
    DateTime? LastRunTimeUtc,
    string? LastResult);
