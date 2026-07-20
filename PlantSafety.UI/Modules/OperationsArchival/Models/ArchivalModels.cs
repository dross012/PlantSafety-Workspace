// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-20
// FILE DESCRIPTION: Client-side DTOs for the Operations.Archival module. Mirror
//   PlantSafety.Application.Operations.Archival.Models.* returned by
//   PlantSafety.Api.Operations.Archival.Controllers.V1.OperationsArchivalV1Controller.
//   Surfaces the real Phase 7 Governance retention/purge automation
//   (74-retention-archival.sql) and Phase 18 (Invoke-PlantSafetyPurge.ps1 Task Scheduler).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.OperationsArchival.Models;

/// <summary>One of the 5 live retention/purge stored procedures.</summary>
public enum PurgeTarget
{
    /// <summary>PlantSafety.AuditDB.dbo.sp_AuditLog_Purge.</summary>
    AuditLog = 0,

    /// <summary>PlantSafety.ErrorDB.dbo.sp_ErrorLog_Purge -- expected to log "Blocked" (TR_ErrorLog_NoDelete).</summary>
    ErrorLog = 1,

    /// <summary>PlantSafety.IntegrationDB.Integration.sp_IntegrationLogEntry_Purge.</summary>
    IntegrationLogEntry = 2,

    /// <summary>PlantSafety.IntegrationDB.Integration.sp_ReplayRecord_Purge.</summary>
    ReplayRecord = 3,

    /// <summary>PlantSafety.DiagnosticsDB.Diagnostics.sp_DiagnosticEvent_Purge.</summary>
    DiagnosticEvent = 4
}

/// <summary>Static metadata describing a single live purge target.</summary>
public sealed class PurgeTargetDto
{
    /// <summary>Gets the purge target selector.</summary>
    public PurgeTarget Target { get; init; }

    /// <summary>Gets the Archival.TierPolicy.Zone key.</summary>
    public string Zone { get; init; } = string.Empty;

    /// <summary>Gets the PlantSafety.* database the purge procedure runs against.</summary>
    public string DatabaseName { get; init; } = string.Empty;

    /// <summary>Gets the fully schema-qualified stored procedure name.</summary>
    public string ProcedureName { get; init; } = string.Empty;

    /// <summary>Gets a human-readable description, including any known caveats.</summary>
    public string Description { get; init; } = string.Empty;
}

/// <summary>Real, synchronous outcome of triggering a single retention purge stored procedure.</summary>
public sealed class PurgeTriggerResultDto
{
    /// <summary>Gets the purge target that was triggered.</summary>
    public PurgeTarget Target { get; init; }

    /// <summary>Gets the Archival.TierPolicy.Zone this run applies to.</summary>
    public string Zone { get; init; } = string.Empty;

    /// <summary>Gets "Completed" | "Blocked" | "Failed" | "Unknown".</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the Archival.ArchivalJob row id logged by the procedure, if found.</summary>
    public Guid? ArchivalJobId { get; init; }

    /// <summary>Gets when the procedure run started.</summary>
    public DateTime? StartedUtc { get; init; }

    /// <summary>Gets when the procedure run completed.</summary>
    public DateTime? CompletedUtc { get; init; }

    /// <summary>Gets human-readable outcome detail.</summary>
    public string? Detail { get; init; }
}

/// <summary>Projection of a single Archival.ArchivalJob row (purge run history/status).</summary>
public sealed class ArchivalJobDto
{
    /// <summary>Gets the job's primary key.</summary>
    public Guid ArchivalJobId { get; init; }

    /// <summary>Gets the dataset id (equal to ArchivalJobId for these system-generated purge runs).</summary>
    public Guid DatasetId { get; init; }

    /// <summary>Gets the tenant id (system actor for these platform-level purge runs).</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the Archival.TierPolicy.Zone this run applies to.</summary>
    public string PartitionKey { get; init; } = string.Empty;

    /// <summary>Gets the source tier (always "Hot" for these targets).</summary>
    public string SourceTier { get; init; } = string.Empty;

    /// <summary>Gets the target tier (always "Purged" for these targets).</summary>
    public string TargetTier { get; init; } = string.Empty;

    /// <summary>Gets "Completed" | "Blocked" | "Failed".</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets when the run started.</summary>
    public DateTime StartedAtUtc { get; init; }

    /// <summary>Gets when the run completed, or null if still in flight.</summary>
    public DateTime? CompletedAtUtc { get; init; }

    /// <summary>Gets the outcome detail (row count purged, block reason, or failure message).</summary>
    public string? FailureReason { get; init; }

    /// <summary>Gets the correlation id.</summary>
    public Guid? CorrelationId { get; init; }
}

/// <summary>Projection of a single Archival.TierPolicy row (retention window settings, read-only).</summary>
public sealed class TierPolicyDto
{
    /// <summary>Gets the policy's primary key.</summary>
    public Guid TierPolicyId { get; init; }

    /// <summary>Gets the zone key.</summary>
    public string Zone { get; init; } = string.Empty;

    /// <summary>Gets the source tier.</summary>
    public string SourceTier { get; init; } = string.Empty;

    /// <summary>Gets the target tier.</summary>
    public string TargetTier { get; init; } = string.Empty;

    /// <summary>Gets the retention window, in days, before rows transition from SourceTier to TargetTier.</summary>
    public int TransitionAfterDays { get; init; }

    /// <summary>Gets when the policy row was created.</summary>
    public DateTime CreatedUtc { get; init; }
}
