// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.FacilityTrafficMonitoring
//  Layer:          Domain
//  File:           FacilityTrafficMonitoringEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for FacilityTrafficMonitoring domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.FacilityTrafficMonitoring.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for FacilityTrafficMonitoring domain events.</summary>
public readonly record struct FacilityTrafficMonitoringEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a FacilityTrafficMonitoringEventId wrapping a non-empty GUID.</summary>
    public FacilityTrafficMonitoringEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("FacilityTrafficMonitoringEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new FacilityTrafficMonitoringEventId using Uuid7.</summary>
    public static FacilityTrafficMonitoringEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}