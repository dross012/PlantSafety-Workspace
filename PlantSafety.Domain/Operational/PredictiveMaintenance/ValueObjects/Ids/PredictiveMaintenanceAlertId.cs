// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Domain
//  File:           PredictiveMaintenanceAlertId.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the PredictiveMaintenanceAlert
//                  aggregate root. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PredictiveMaintenance.ValueObjects.Ids;

/// <summary>Strong-typed identity for the PredictiveMaintenanceAlert aggregate root.</summary>
public readonly record struct PredictiveMaintenanceAlertId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PredictiveMaintenanceAlertId wrapping a non-empty GUID.</summary>
    public PredictiveMaintenanceAlertId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PredictiveMaintenanceAlertId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PredictiveMaintenanceAlertId using Uuid7.</summary>
    public static PredictiveMaintenanceAlertId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PredictiveMaintenanceAlertId.</summary>
    public static PredictiveMaintenanceAlertId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
