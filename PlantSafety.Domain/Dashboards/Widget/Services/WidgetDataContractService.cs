// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           WidgetDataContractService.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Pure domain service validating the shape of the normalized widget data
//                  contract, per "3. Data Model for Widgets: Input: normalized series or tabular
//                  data: Series[] (name, values, timestamps), Categories[], Metrics[] (value,
//                  label, severity)". Contains no infrastructure logic — the Infrastructure layer
//                  is responsible for actually producing this shape from module read models.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Dashboards.Widget.Enums;

namespace PlantSafety.Domain.Dashboards.Widget.Services;

/// <summary>Domain service validating that widget data contracts are structurally compatible with a widget type.</summary>
public sealed class WidgetDataContractService : IDomainService
{
    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="seriesCount"/>, <paramref name="categoryCount"/>,
    /// and <paramref name="metricCount"/> together describe a data shape that is renderable by
    /// <paramref name="type"/> (e.g. a Gauge/KpiCard requires exactly one metric and no series).
    /// </summary>
    public bool IsCompatibleDataShape(WidgetType type, int seriesCount, int categoryCount, int metricCount)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (type.Equals(WidgetType.Gauge) || type.Equals(WidgetType.KpiCard))
            return metricCount == 1 && seriesCount == 0;

        if (type.Equals(WidgetType.LineChart) || type.Equals(WidgetType.TrendGraph) || type.Equals(WidgetType.Timeline))
            return seriesCount >= 1;

        if (type.Equals(WidgetType.BarChart) || type.Equals(WidgetType.PieChart))
            return categoryCount >= 1;

        if (type.Equals(WidgetType.Table))
            return categoryCount >= 1 || seriesCount >= 1;

        if (type.Equals(WidgetType.Heatmap) || type.Equals(WidgetType.Map))
            return categoryCount >= 1 || seriesCount >= 1;

        return false;
    }
}
