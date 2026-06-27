// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Domain
//  File:           PermitsSnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Permits aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Permits.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Permits"/> aggregate for fast rehydration.</summary>
public sealed record PermitsSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public PermitsId PermitsId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public PermitsStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
