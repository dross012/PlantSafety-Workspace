// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
//  Layer:          Domain
//  File:           EnvironmentalHealthMonitoringId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the EnvironmentalHealthMonitoring aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the EnvironmentalHealthMonitoring aggregate root.</summary>
public readonly record struct EnvironmentalHealthMonitoringId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a EnvironmentalHealthMonitoringId wrapping a non-empty GUID.</summary>
    public EnvironmentalHealthMonitoringId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EnvironmentalHealthMonitoringId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EnvironmentalHealthMonitoringId using Uuid7.</summary>
    public static EnvironmentalHealthMonitoringId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a EnvironmentalHealthMonitoringId.</summary>
    public static EnvironmentalHealthMonitoringId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a EnvironmentalHealthMonitoringId.</summary>
    public static EnvironmentalHealthMonitoringId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}