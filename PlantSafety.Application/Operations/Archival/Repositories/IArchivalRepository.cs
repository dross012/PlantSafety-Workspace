// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Archival
//  Layer:          Application
//  File:           IArchivalRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Repository abstraction over the real Phase7 Governance retention/purge
//                  automation (sp_AuditLog_Purge, sp_ErrorLog_Purge,
//                  sp_IntegrationLogEntry_Purge, sp_ReplayRecord_Purge,
//                  sp_DiagnosticEvent_Purge -- 74-retention-archival.sql) and the
//                  [PlantSafety.ArchiveDB].Archival.ArchivalJob / Archival.TierPolicy
//                  tables it reads/writes. These 5 procedures already run unattended daily
//                  via Windows Task Scheduler (Phase 18, Invoke-PlantSafetyPurge.ps1) since
//                  SQL Server Agent is disabled on this Express instance -- this repository
//                  lets an operator run the SAME approved operation on demand. Implemented
//                  in the Infrastructure layer. Additive-only evolution.
// ============================================================================
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PlantSafety.Application.Operations.Archival.Models;

namespace PlantSafety.Application.Operations.Archival.Repositories;

/// <summary>Repository over the real Phase 7 Governance retention/purge automation.</summary>
public interface IArchivalRepository
{
    /// <summary>
    /// Returns static metadata for the 5 live purge targets (database, procedure, TierPolicy
    /// zone). No I/O -- this is a fixed, known inventory documented in
    /// 74-retention-archival.sql, not something discovered at runtime.
    /// </summary>
    IReadOnlyList<PurgeTargetDto> GetPurgeTargets();

    /// <summary>
    /// Executes the purge stored procedure for <paramref name="target"/>, or for all 5
    /// targets in Zone order if <paramref name="target"/> is null. Each procedure is invoked
    /// synchronously via a direct <c>EXEC</c> -- these are fast, bounded DELETE statements
    /// that already log their own definitive Completed/Blocked/Failed outcome, so the caller
    /// receives the real result immediately rather than a "Started" acknowledgement.
    /// </summary>
    Task<IReadOnlyList<PurgeTriggerResultDto>> TriggerPurgeAsync(PurgeTarget? target, CancellationToken cancellationToken);

    /// <summary>
    /// Returns recent purge run history from [PlantSafety.ArchiveDB].Archival.ArchivalJob,
    /// newest first, optionally filtered to a single Zone.
    /// </summary>
    Task<IReadOnlyList<ArchivalJobDto>> GetPurgeStatusAsync(string? zone, int top, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the current retention windows from
    /// [PlantSafety.ArchiveDB].Archival.TierPolicy (read-only -- no settings editor in this
    /// module).
    /// </summary>
    Task<IReadOnlyList<TierPolicyDto>> GetRetentionSettingsAsync(CancellationToken cancellationToken);
}
