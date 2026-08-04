// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.LoadSecurementChecks
//  Layer:          Domain
//  File:           LoadSecurementCheckSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for LoadSecurementCheck aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Snapshots;

/// <summary>Immutable snapshot of a <see cref="LoadSecurementCheck"/> aggregate for fast rehydration.</summary>
public sealed record LoadSecurementCheckSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public LoadSecurementCheckId LoadSecurementCheckId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public LoadSecurementCheckStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
