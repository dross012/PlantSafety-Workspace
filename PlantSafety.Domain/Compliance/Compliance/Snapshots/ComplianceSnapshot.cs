// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Domain
//  File:           ComplianceSnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Compliance aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Compliance.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Compliance"/> aggregate for fast rehydration.</summary>
public sealed record ComplianceSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public ComplianceId ComplianceId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public ComplianceStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
