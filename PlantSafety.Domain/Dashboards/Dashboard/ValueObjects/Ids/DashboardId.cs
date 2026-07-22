// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the Dashboard aggregate root.
//                  Grounded in "Dashboard Design Architecture, 1. Dashboard: Has: Id, Name,
//                  Description, Role, DefaultFilters, WidgetLayout".
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the <see cref="Aggregates.Dashboard"/> aggregate root.</summary>
public readonly record struct DashboardId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a DashboardId wrapping a non-empty GUID.</summary>
    public DashboardId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DashboardId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new DashboardId using Uuid7.</summary>
    public static DashboardId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a DashboardId.</summary>
    public static DashboardId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a DashboardId.</summary>
    public static DashboardId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
