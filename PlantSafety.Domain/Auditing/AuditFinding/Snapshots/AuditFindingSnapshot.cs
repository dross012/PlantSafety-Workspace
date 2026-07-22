// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditFinding
//  Layer:          Domain
//  File:           AuditFindingSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for AuditFinding aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditFinding.Snapshots;

/// <summary>Immutable snapshot of a <see cref="AuditFinding"/> aggregate for fast rehydration.</summary>
public sealed record AuditFindingSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public AuditFindingId AuditFindingId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public AuditFindingStatus Status { get; init; }

    /// <summary>Identity of the parent AuditEngagement.</summary>
    public Guid AuditEngagementId { get; init; }

    /// <summary>Severity classification at snapshot time.</summary>
    public string Severity { get; init; } = "Medium";

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}