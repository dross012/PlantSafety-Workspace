// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.Carriers
//  Layer:          Domain
//  File:           CarrierSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Carrier aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.Carriers.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Carrier"/> aggregate for fast rehydration.</summary>
public sealed record CarrierSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public CarrierId CarrierId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public CarrierStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
