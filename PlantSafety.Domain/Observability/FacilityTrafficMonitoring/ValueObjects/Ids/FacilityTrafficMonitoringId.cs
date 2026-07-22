// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.FacilityTrafficMonitoring
//  Layer:          Domain
//  File:           FacilityTrafficMonitoringId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for the FacilityTrafficMonitoring aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.FacilityTrafficMonitoring.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the FacilityTrafficMonitoring aggregate root.</summary>
public readonly record struct FacilityTrafficMonitoringId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a FacilityTrafficMonitoringId wrapping a non-empty GUID.</summary>
    public FacilityTrafficMonitoringId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("FacilityTrafficMonitoringId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new FacilityTrafficMonitoringId using Uuid7.</summary>
    public static FacilityTrafficMonitoringId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a FacilityTrafficMonitoringId.</summary>
    public static FacilityTrafficMonitoringId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a FacilityTrafficMonitoringId.</summary>
    public static FacilityTrafficMonitoringId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}