// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportVehicles
//  Layer:          Domain
//  File:           TransportVehicleSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for TransportVehicle aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Snapshots;

/// <summary>Immutable snapshot of a <see cref="TransportVehicle"/> aggregate for fast rehydration.</summary>
public sealed record TransportVehicleSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public TransportVehicleId TransportVehicleId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public TransportVehicleStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
