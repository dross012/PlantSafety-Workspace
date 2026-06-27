// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Domain
//  File:           AuditingSnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Auditing aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Auditing.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Auditing"/> aggregate for fast rehydration.</summary>
public sealed record AuditingSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public AuditingId AuditingId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public AuditingStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
