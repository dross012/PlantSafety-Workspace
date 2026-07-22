// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.PPEBehavioralMonitoring
//  Layer:          Domain
//  File:           PPEBehavioralMonitoringEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for PPEBehavioralMonitoring domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.PPEBehavioralMonitoring.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for PPEBehavioralMonitoring domain events.</summary>
public readonly record struct PPEBehavioralMonitoringEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PPEBehavioralMonitoringEventId wrapping a non-empty GUID.</summary>
    public PPEBehavioralMonitoringEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PPEBehavioralMonitoringEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PPEBehavioralMonitoringEventId using Uuid7.</summary>
    public static PPEBehavioralMonitoringEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}