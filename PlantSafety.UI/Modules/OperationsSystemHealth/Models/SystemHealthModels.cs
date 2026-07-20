// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-20
// FILE DESCRIPTION: Client-side DTOs for the Operations.SystemHealth module. Mirror
//   PlantSafety.Application.Operations.SystemHealth.Models.* returned by
//   PlantSafety.Api.Operations.SystemHealth.Controllers.V1
//   .OperationsSystemHealthV1Controller. Reuses the client-side DTOs already defined by
//   the sibling Operations.* modules for nested sections rather than duplicating fields.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.OperationsArchival.Models;
using PlantSafety.UI.Modules.OperationsBackup.Models;
using PlantSafety.UI.Modules.OperationsConfiguration.Models;
using PlantSafety.UI.Modules.OperationsMonitoring.Models;
using PlantSafety.UI.Modules.SchemaGuard.Models;

namespace PlantSafety.UI.Modules.OperationsSystemHealth.Models;

/// <summary>
/// Generic client-side wrapper mirroring Application's SystemHealthSectionDto{T} --
/// each section of the rollup report is independently marked available/unavailable.
/// </summary>
public sealed class SystemHealthSectionDto<T>
{
    /// <summary>Gets a value indicating whether this section's data was retrieved successfully.</summary>
    public bool IsAvailable { get; init; }

    /// <summary>Gets the section payload. Default when <see cref="IsAvailable"/> is false.</summary>
    public T? Data { get; init; }

    /// <summary>Gets the sanitized, user-safe reason this section is unavailable, or null when available.</summary>
    public string? UnavailableReason { get; init; }
}

/// <summary>The unified System Health rollup report.</summary>
public sealed class SystemHealthReportDto
{
    /// <summary>Gets when this report was assembled.</summary>
    public DateTime GeneratedUtc { get; init; }

    /// <summary>Gets the running PlantSafety.Api assembly version.</summary>
    public string AppVersion { get; init; } = string.Empty;

    /// <summary>Gets per-database backup health.</summary>
    public SystemHealthSectionDto<List<BackupHealthDto>> BackupHealth { get; init; } = new();

    /// <summary>Gets per-database schema-drift health.</summary>
    public SystemHealthSectionDto<List<SchemaDatabaseHealthDto>> SchemaHealth { get; init; } = new();

    /// <summary>Gets the aggregated /health/ready readiness report.</summary>
    public SystemHealthSectionDto<DatabaseHealthReportDto> DatabaseHealth { get; init; } = new();

    /// <summary>Gets the combined DB-performance/error-count summary.</summary>
    public SystemHealthSectionDto<MonitoringSummaryDto> MonitoringSummary { get; init; } = new();

    /// <summary>Gets recent purge run history.</summary>
    public SystemHealthSectionDto<List<ArchivalJobDto>> PurgeStatus { get; init; } = new();

    /// <summary>Gets the count of AuditLog rows written in the last 24 hours.</summary>
    public SystemHealthSectionDto<int> RecentAuditActivityCount { get; init; } = new();

    /// <summary>Gets every feature flag across every known database.</summary>
    public SystemHealthSectionDto<List<FeatureFlagDto>> FeatureFlags { get; init; } = new();

    /// <summary>Gets live status of the Phase 18 Windows Task Scheduler automation jobs.</summary>
    public SystemHealthSectionDto<List<TaskSchedulerJobStatusDto>> TaskSchedulerJobs { get; init; } = new();

    /// <summary>Gets the SQL Server version/edition of the primary tenant-scoped connection.</summary>
    public SystemHealthSectionDto<SystemHealthDatabaseVersionDto> DatabaseVersion { get; init; } = new();
}

/// <summary>Combined DB-performance/error-count summary, reused from Operations.Monitoring's DTOs.</summary>
public sealed class MonitoringSummaryDto
{
    /// <summary>Gets the number of ErrorLog rows logged in the last 24 hours.</summary>
    public int RecentErrorCount { get; init; }

    /// <summary>Gets, of <see cref="RecentErrorCount"/>, how many were high severity (&gt;= 17).</summary>
    public int HighSeverityErrorCount { get; init; }

    /// <summary>Gets the most recent error rows.</summary>
    public List<ErrorLogItemDto> RecentErrors { get; init; } = [];

    /// <summary>Gets the top queries by average CPU time for the primary tenant database.</summary>
    public List<QueryPerformanceItemDto> TopQueriesByCpu { get; init; } = [];
}

/// <summary>Live status of a single registered Windows Task Scheduler automation task.</summary>
public sealed class TaskSchedulerJobStatusDto
{
    /// <summary>Gets the exact registered task name.</summary>
    public string TaskName { get; init; } = string.Empty;

    /// <summary>Gets "Backup" | "Purge" | "DatabaseSizeLog".</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Gets "Ready" | "Running" | "Disabled" | "Unknown" | "Unavailable".</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the task's last run time, if parsed successfully.</summary>
    public DateTime? LastRunTimeUtc { get; init; }

    /// <summary>Gets the task's last result code, if parsed successfully.</summary>
    public string? LastResult { get; init; }
}

/// <summary>SQL Server version/edition of the primary tenant-scoped database connection.</summary>
public sealed class SystemHealthDatabaseVersionDto
{
    /// <summary>Gets SERVERPROPERTY('ProductVersion').</summary>
    public string ProductVersion { get; init; } = string.Empty;

    /// <summary>Gets SERVERPROPERTY('ProductLevel').</summary>
    public string? ProductLevel { get; init; }

    /// <summary>Gets SERVERPROPERTY('Edition').</summary>
    public string Edition { get; init; } = string.Empty;

    /// <summary>Gets SERVERPROPERTY('EngineEdition') numeric code.</summary>
    public int EngineEdition { get; init; }
}
