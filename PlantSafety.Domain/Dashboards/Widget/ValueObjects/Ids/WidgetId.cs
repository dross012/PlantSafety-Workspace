// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           WidgetId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the Widget aggregate root. Grounded
//                  in PlantSafety_Program_Structure_extract.txt "Dashboard Design Architecture,
//                  2. Widget: Has: WidgetId...".
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Dashboards.Widget.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the <see cref="Aggregates.Widget"/> aggregate root.</summary>
public readonly record struct WidgetId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a WidgetId wrapping a non-empty GUID.</summary>
    public WidgetId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WidgetId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new WidgetId using Uuid7.</summary>
    public static WidgetId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a WidgetId.</summary>
    public static WidgetId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a WidgetId.</summary>
    public static WidgetId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
