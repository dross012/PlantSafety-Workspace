// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.MachineProcessMonitoring
//  Layer:          Domain
//  File:           MachineProcessMonitoringEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for MachineProcessMonitoring domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.MachineProcessMonitoring.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for MachineProcessMonitoring domain events.</summary>
public readonly record struct MachineProcessMonitoringEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a MachineProcessMonitoringEventId wrapping a non-empty GUID.</summary>
    public MachineProcessMonitoringEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MachineProcessMonitoringEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new MachineProcessMonitoringEventId using Uuid7.</summary>
    public static MachineProcessMonitoringEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}