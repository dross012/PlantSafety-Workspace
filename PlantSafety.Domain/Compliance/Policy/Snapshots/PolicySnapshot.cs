// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Domain
//  File:           PolicySnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Policy aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Policy.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Policy"/> aggregate for fast rehydration.</summary>
public sealed record PolicySnapshot
{
    /// <summary>Aggregate identity.</summary>
    public PolicyId PolicyId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public PolicyStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
