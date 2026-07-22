// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.StopWorkEvent
//  Layer:          Domain
//  File:           StopWorkEventEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for StopWorkEvent domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.StopWorkEvent.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for StopWorkEvent domain events.</summary>
public readonly record struct StopWorkEventEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a StopWorkEventEventId wrapping a non-empty GUID.</summary>
    public StopWorkEventEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("StopWorkEventEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new StopWorkEventEventId using Uuid7.</summary>
    public static StopWorkEventEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}