// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.MachineProcessMonitoring
//  Layer:          Domain
//  File:           MachineProcessMonitoringId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for the MachineProcessMonitoring aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.MachineProcessMonitoring.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the MachineProcessMonitoring aggregate root.</summary>
public readonly record struct MachineProcessMonitoringId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a MachineProcessMonitoringId wrapping a non-empty GUID.</summary>
    public MachineProcessMonitoringId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MachineProcessMonitoringId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new MachineProcessMonitoringId using Uuid7.</summary>
    public static MachineProcessMonitoringId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a MachineProcessMonitoringId.</summary>
    public static MachineProcessMonitoringId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a MachineProcessMonitoringId.</summary>
    public static MachineProcessMonitoringId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}