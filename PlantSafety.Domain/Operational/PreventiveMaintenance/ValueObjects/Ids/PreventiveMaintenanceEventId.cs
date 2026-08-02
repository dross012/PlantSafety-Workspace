// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Domain
//  File:           PreventiveMaintenanceEventId.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for PreventiveMaintenanceTask domain
//                  events. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PreventiveMaintenance.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for PreventiveMaintenanceTask domain events.</summary>
public readonly record struct PreventiveMaintenanceEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PreventiveMaintenanceEventId wrapping a non-empty GUID.</summary>
    public PreventiveMaintenanceEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PreventiveMaintenanceEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PreventiveMaintenanceEventId using Uuid7.</summary>
    public static PreventiveMaintenanceEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
