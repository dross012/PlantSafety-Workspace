// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement
//  Layer:          Domain
//  File:           AuditEngagementSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for AuditEngagement aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditEngagement.Snapshots;

/// <summary>Immutable snapshot of a <see cref="AuditEngagement"/> aggregate for fast rehydration.</summary>
public sealed record AuditEngagementSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public AuditEngagementId AuditEngagementId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public AuditEngagementStatus Status { get; init; }

    /// <summary>Audit discipline code.</summary>
    public string DisciplineType { get; init; } = string.Empty;

    /// <summary>Computed compliance score at snapshot time, when available.</summary>
    public decimal? ComplianceScore { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}