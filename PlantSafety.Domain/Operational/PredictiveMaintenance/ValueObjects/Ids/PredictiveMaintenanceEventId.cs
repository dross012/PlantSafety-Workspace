// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Domain
//  File:           PredictiveMaintenanceEventId.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for PredictiveMaintenanceAlert domain
//                  events. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PredictiveMaintenance.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for PredictiveMaintenanceAlert domain events.</summary>
public readonly record struct PredictiveMaintenanceEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PredictiveMaintenanceEventId wrapping a non-empty GUID.</summary>
    public PredictiveMaintenanceEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PredictiveMaintenanceEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PredictiveMaintenanceEventId using Uuid7.</summary>
    public static PredictiveMaintenanceEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
