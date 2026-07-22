// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardFilter.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Value Object representing a single default filter applied to a Dashboard, per
//                  "1. Dashboard: Has: ... DefaultFilters" and "3. Dashboard Service Layer:
//                  Applies role-based filters (plant, line, department, shift)".
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Dashboards.Dashboard.ValueObjects;

/// <summary>Immutable key/value default filter attached to a <see cref="Aggregates.Dashboard"/>.</summary>
public readonly record struct DashboardFilter
{
    /// <summary>Filter dimension key (e.g. "Plant", "Line", "Department", "Shift").</summary>
    public string Key { get; }

    /// <summary>Filter value, serialized as a normalized string.</summary>
    public string Value { get; }

    private DashboardFilter(string key, string value)
    {
        Key = key;
        Value = value;
    }

    /// <summary>Creates a normalized <see cref="DashboardFilter"/>. Rejects null/empty/whitespace keys.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null, empty, or whitespace.</exception>
    public static DashboardFilter Create(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("DashboardFilter.Key must not be null or whitespace.", nameof(key));

        return new DashboardFilter(key.Trim(), (value ?? string.Empty).Trim());
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Key}={Value}";
}
