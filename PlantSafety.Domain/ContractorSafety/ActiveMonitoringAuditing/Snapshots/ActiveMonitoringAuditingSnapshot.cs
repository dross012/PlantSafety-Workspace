// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.ActiveMonitoringAuditing
//  Layer:          Domain
//  File:           ActiveMonitoringAuditingSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for ActiveMonitoringAuditing aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Snapshots;

/// <summary>Immutable snapshot of a <see cref="ActiveMonitoringAuditing"/> aggregate for fast rehydration.</summary>
public sealed record ActiveMonitoringAuditingSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public ActiveMonitoringAuditingId ActiveMonitoringAuditingId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public ActiveMonitoringAuditingStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}