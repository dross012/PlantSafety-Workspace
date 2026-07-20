// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.SystemHealth
//  Layer:          Application
//  File:           ISystemHealthRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Repository abstraction over the two genuinely new System Health data
//                  sources that no other Operations.* module already surfaces: SQL Server
//                  version/edition of the primary tenant connection, and live Windows
//                  Task Scheduler status for the Phase 18 automation jobs. Implemented in
//                  the Infrastructure layer. Additive-only evolution.
// ============================================================================
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PlantSafety.Application.Operations.SystemHealth.Models;

namespace PlantSafety.Application.Operations.SystemHealth.Repositories;

/// <summary>Repository over the System Health module's own new data sources (DB version, Task Scheduler status).</summary>
public interface ISystemHealthRepository
{
    /// <summary>
    /// Retrieves the SQL Server version/edition of the primary tenant-scoped connection
    /// (ISqlConnectionFactory.CreateConnection()) via SERVERPROPERTY.
    /// </summary>
    Task<SystemHealthDatabaseVersionDto> GetDatabaseVersionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the live status of every known Phase 18 Windows Task Scheduler automation
    /// job via schtasks.exe. Returns "Unavailable" status per task (never throws) when
    /// schtasks.exe is missing, the task is not registered, or its output cannot be parsed
    /// -- this is a nice-to-have status indicator, not a hard dependency.
    /// </summary>
    Task<IReadOnlyList<TaskSchedulerJobStatusDto>> GetTaskSchedulerStatusAsync(CancellationToken cancellationToken);
}
