// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessAnomalies
//  Layer:          Domain
//  File:           AccessAnomalyEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for AccessAnomaly domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for AccessAnomaly domain events.</summary>
public readonly record struct AccessAnomalyEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an AccessAnomalyEventId wrapping a non-empty GUID.</summary>
    public AccessAnomalyEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AccessAnomalyEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AccessAnomalyEventId using Uuid7.</summary>
    public static AccessAnomalyEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
