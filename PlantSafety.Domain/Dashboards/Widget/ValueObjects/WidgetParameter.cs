// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           WidgetParameter.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Value Object representing a single widget configuration parameter, per
//                  "2. Widget: Has: ... Parameters (time range, plant, module, thresholds)".
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Dashboards.Widget.ValueObjects;

/// <summary>Immutable key/value configuration parameter attached to a <see cref="Aggregates.Widget"/>.</summary>
public readonly record struct WidgetParameter
{
    /// <summary>Parameter key (e.g. "TimeRange", "Plant", "Threshold").</summary>
    public string Key { get; }

    /// <summary>Parameter value, serialized as a normalized string.</summary>
    public string Value { get; }

    private WidgetParameter(string key, string value)
    {
        Key = key;
        Value = value;
    }

    /// <summary>Creates a normalized <see cref="WidgetParameter"/>. Rejects null/empty/whitespace keys.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null, empty, or whitespace.</exception>
    public static WidgetParameter Create(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("WidgetParameter.Key must not be null or whitespace.", nameof(key));

        return new WidgetParameter(key.Trim(), (value ?? string.Empty).Trim());
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Key}={Value}";
}
