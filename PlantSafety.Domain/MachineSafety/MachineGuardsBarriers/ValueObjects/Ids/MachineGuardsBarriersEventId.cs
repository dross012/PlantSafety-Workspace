// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.MachineGuardsBarriers
//  Layer:          Domain
//  File:           MachineGuardsBarriersEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for MachineGuardsBarriers domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for MachineGuardsBarriers domain events.</summary>
public readonly record struct MachineGuardsBarriersEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a MachineGuardsBarriersEventId wrapping a non-empty GUID.</summary>
    public MachineGuardsBarriersEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MachineGuardsBarriersEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new MachineGuardsBarriersEventId using Uuid7.</summary>
    public static MachineGuardsBarriersEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}