// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Domain
//  File:           RegulatoryCalendarEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.RegulatoryCalendar.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for RegulatoryCalendar domain events.</summary>
public readonly record struct RegulatoryCalendarEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a RegulatoryCalendarEventId wrapping a non-empty GUID.</summary>
    public RegulatoryCalendarEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RegulatoryCalendarEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new RegulatoryCalendarEventId using Uuid7.</summary>
    public static RegulatoryCalendarEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
