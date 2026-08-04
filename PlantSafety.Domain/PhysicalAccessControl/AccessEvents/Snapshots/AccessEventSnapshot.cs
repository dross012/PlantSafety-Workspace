// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessEvents
//  Layer:          Domain
//  File:           AccessEventSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for AccessEvent aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Snapshots;

/// <summary>Immutable snapshot of an <see cref="AccessEvent"/> aggregate for fast rehydration.</summary>
public sealed record AccessEventSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public AccessEventId AccessEventId { get; init; }

    /// <summary>Processing status at snapshot time.</summary>
    public AccessEventStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user/system actor who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
