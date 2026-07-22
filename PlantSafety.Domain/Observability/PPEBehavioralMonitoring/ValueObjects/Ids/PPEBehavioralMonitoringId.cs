// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.PPEBehavioralMonitoring
//  Layer:          Domain
//  File:           PPEBehavioralMonitoringId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for the PPEBehavioralMonitoring aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.PPEBehavioralMonitoring.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the PPEBehavioralMonitoring aggregate root.</summary>
public readonly record struct PPEBehavioralMonitoringId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PPEBehavioralMonitoringId wrapping a non-empty GUID.</summary>
    public PPEBehavioralMonitoringId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PPEBehavioralMonitoringId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PPEBehavioralMonitoringId using Uuid7.</summary>
    public static PPEBehavioralMonitoringId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PPEBehavioralMonitoringId.</summary>
    public static PPEBehavioralMonitoringId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PPEBehavioralMonitoringId.</summary>
    public static PPEBehavioralMonitoringId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}