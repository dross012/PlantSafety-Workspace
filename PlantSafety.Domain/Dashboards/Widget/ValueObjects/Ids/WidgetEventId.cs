// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           WidgetEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for Widget domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Dashboards.Widget.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Widget domain events.</summary>
public readonly record struct WidgetEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a WidgetEventId wrapping a non-empty GUID.</summary>
    public WidgetEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WidgetEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new WidgetEventId using Uuid7.</summary>
    public static WidgetEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
