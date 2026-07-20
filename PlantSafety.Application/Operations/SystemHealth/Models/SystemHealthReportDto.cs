// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.SystemHealth
//  Layer:          Application
//  File:           SystemHealthReportDto.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unified rollup report for the System Health Dashboard -- the final
//                  deliverable of the 10-step "bring C# up to the SQL Server hardening
//                  standard" roadmap. Composes the existing DTOs from Operations.Backup,
//                  Operations.SchemaGuard, Operations.Monitoring, Operations.Archival,
//                  Operations.AuditViewer, and Operations.Configuration as nested
//                  properties rather than flattening/duplicating their fields, plus the
//                  two genuinely new pieces (app version, DB version) and Task Scheduler
//                  job status. Every section is independently resilient -- see
//                  SystemHealthSectionDto{T}. Additive-only evolution.
// ============================================================================
using System;
using System.Collections.Generic;

using PlantSafety.Application.Operations.Archival.Models;
using PlantSafety.Application.Operations.Backup.Models;
using PlantSafety.Application.Operations.Configuration.Models;
using PlantSafety.Application.Operations.Monitoring.Models;
using PlantSafety.Application.Operations.SchemaGuard.Models;

namespace PlantSafety.Application.Operations.SystemHealth.Models;

/// <summary>Unified rollup report for the Operations.SystemHealth dashboard.</summary>
/// <param name="GeneratedUtc">When this report was assembled.</param>
/// <param name="AppVersion">The running PlantSafety.Api assembly version (System.Reflection.AssemblyName.Version).</param>
/// <param name="BackupHealth">Per-database backup health, from Operations.Backup's GetBackupHealthQuery.</param>
/// <param name="SchemaHealth">Per-database schema-drift health, from Operations.SchemaGuard's GetSchemaHealthQuery.</param>
/// <param name="DatabaseHealth">The aggregated /health/ready readiness report, from Operations.Monitoring's GetDatabaseHealthReportQuery.</param>
/// <param name="MonitoringSummary">Combined DB-performance/error-count summary, from Operations.Monitoring's GetErrorLogQuery/GetQueryPerformanceQuery.</param>
/// <param name="PurgeStatus">Recent purge run history, from Operations.Archival's GetPurgeStatusQuery.</param>
/// <param name="RecentAuditActivityCount">Count of AuditLog rows in the last 24 hours, from Operations.AuditViewer's SearchAuditLogQuery.</param>
/// <param name="FeatureFlags">Every feature flag across every known database, from Operations.Configuration's GetAllFeatureFlagsQuery.</param>
/// <param name="TaskSchedulerJobs">Live status of the Phase 18 Windows Task Scheduler automation jobs.</param>
/// <param name="DatabaseVersion">SQL Server version/edition of the primary tenant-scoped connection.</param>
public sealed record SystemHealthReportDto(
    DateTime GeneratedUtc,
    string AppVersion,
    SystemHealthSectionDto<IReadOnlyList<BackupHealthDto>> BackupHealth,
    SystemHealthSectionDto<IReadOnlyList<SchemaDatabaseHealthDto>> SchemaHealth,
    SystemHealthSectionDto<DatabaseHealthReportDto> DatabaseHealth,
    SystemHealthSectionDto<MonitoringSummaryDto> MonitoringSummary,
    SystemHealthSectionDto<IReadOnlyList<ArchivalJobDto>> PurgeStatus,
    SystemHealthSectionDto<int> RecentAuditActivityCount,
    SystemHealthSectionDto<IReadOnlyList<FeatureFlagDto>> FeatureFlags,
    SystemHealthSectionDto<IReadOnlyList<TaskSchedulerJobStatusDto>> TaskSchedulerJobs,
    SystemHealthSectionDto<SystemHealthDatabaseVersionDto> DatabaseVersion);
