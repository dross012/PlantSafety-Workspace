// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.DriverQualifications
//  Layer:          Domain
//  File:           DriverQualificationSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for DriverQualification aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Snapshots;

/// <summary>Immutable snapshot of a <see cref="DriverQualification"/> aggregate for fast rehydration.</summary>
public sealed record DriverQualificationSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public DriverQualificationId DriverQualificationId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public DriverQualificationStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
