// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Domain
//  File:           RegulatoryCalendarId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.RegulatoryCalendar.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the RegulatoryCalendar aggregate root.</summary>
public readonly record struct RegulatoryCalendarId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a RegulatoryCalendarId wrapping a non-empty GUID.</summary>
    public RegulatoryCalendarId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RegulatoryCalendarId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new RegulatoryCalendarId using Uuid7.</summary>
    public static RegulatoryCalendarId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a RegulatoryCalendarId.</summary>
    public static RegulatoryCalendarId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a RegulatoryCalendarId.</summary>
    public static RegulatoryCalendarId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
