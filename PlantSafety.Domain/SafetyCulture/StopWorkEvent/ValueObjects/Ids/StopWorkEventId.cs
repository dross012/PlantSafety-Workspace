// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.StopWorkEvent
//  Layer:          Domain
//  File:           StopWorkEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the StopWorkEvent aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.StopWorkEvent.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the StopWorkEvent aggregate root.</summary>
public readonly record struct StopWorkEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a StopWorkEventId wrapping a non-empty GUID.</summary>
    public StopWorkEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("StopWorkEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new StopWorkEventId using Uuid7.</summary>
    public static StopWorkEventId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a StopWorkEventId.</summary>
    public static StopWorkEventId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a StopWorkEventId.</summary>
    public static StopWorkEventId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}