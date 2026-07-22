// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.MachineGuardsBarriers
//  Layer:          Domain
//  File:           MachineGuardsBarriersId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the MachineGuardsBarriers aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the MachineGuardsBarriers aggregate root.</summary>
public readonly record struct MachineGuardsBarriersId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a MachineGuardsBarriersId wrapping a non-empty GUID.</summary>
    public MachineGuardsBarriersId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MachineGuardsBarriersId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new MachineGuardsBarriersId using Uuid7.</summary>
    public static MachineGuardsBarriersId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a MachineGuardsBarriersId.</summary>
    public static MachineGuardsBarriersId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a MachineGuardsBarriersId.</summary>
    public static MachineGuardsBarriersId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}