// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           WidgetSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Widget aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Dashboards.Widget.Enums;
using PlantSafety.Domain.Dashboards.Widget.ValueObjects.Ids;

namespace PlantSafety.Domain.Dashboards.Widget.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Aggregates.Widget"/> aggregate for fast rehydration.</summary>
public sealed record WidgetSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public WidgetId WidgetId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public WidgetStatus Status { get; init; }

    /// <summary>Widget type code at snapshot time.</summary>
    public string TypeCode { get; init; } = WidgetType.Default.Code;

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }
}
