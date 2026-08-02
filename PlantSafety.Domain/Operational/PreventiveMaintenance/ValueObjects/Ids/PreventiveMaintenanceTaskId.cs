// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Domain
//  File:           PreventiveMaintenanceTaskId.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the PreventiveMaintenanceTask
//                  aggregate root. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PreventiveMaintenance.ValueObjects.Ids;

/// <summary>Strong-typed identity for the PreventiveMaintenanceTask aggregate root.</summary>
public readonly record struct PreventiveMaintenanceTaskId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PreventiveMaintenanceTaskId wrapping a non-empty GUID.</summary>
    public PreventiveMaintenanceTaskId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PreventiveMaintenanceTaskId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PreventiveMaintenanceTaskId using Uuid7.</summary>
    public static PreventiveMaintenanceTaskId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PreventiveMaintenanceTaskId.</summary>
    public static PreventiveMaintenanceTaskId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
