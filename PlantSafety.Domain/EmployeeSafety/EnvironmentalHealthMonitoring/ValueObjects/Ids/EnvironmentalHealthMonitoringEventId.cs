// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
//  Layer:          Domain
//  File:           EnvironmentalHealthMonitoringEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for EnvironmentalHealthMonitoring domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for EnvironmentalHealthMonitoring domain events.</summary>
public readonly record struct EnvironmentalHealthMonitoringEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a EnvironmentalHealthMonitoringEventId wrapping a non-empty GUID.</summary>
    public EnvironmentalHealthMonitoringEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EnvironmentalHealthMonitoringEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EnvironmentalHealthMonitoringEventId using Uuid7.</summary>
    public static EnvironmentalHealthMonitoringEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}