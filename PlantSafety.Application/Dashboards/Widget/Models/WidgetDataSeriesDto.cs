// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Normalized widget data-contract DTOs, per "3. Data Model for Widgets".
// SchemaVersion: 1

namespace PlantSafety.Application.Dashboards.Widget.Models;

/// <summary>A single named data series with parallel value/timestamp arrays (e.g. a LineChart trend).</summary>
public sealed record WidgetDataSeriesDto
{
    /// <summary>Series name (e.g. "TRIR", "DART").</summary>
    public required string Name { get; init; }

    /// <summary>Series values, parallel to <see cref="Timestamps"/>.</summary>
    public required IReadOnlyList<double> Values { get; init; }

    /// <summary>Series timestamps (UTC), parallel to <see cref="Values"/>.</summary>
    public required IReadOnlyList<DateTime> Timestamps { get; init; }
}

/// <summary>A single categorical bucket (e.g. a BarChart/PieChart/Heatmap category).</summary>
public sealed record WidgetDataCategoryDto
{
    /// <summary>Category label (e.g. "Mechanical", "Electrical").</summary>
    public required string Label { get; init; }

    /// <summary>Category value/count.</summary>
    public required double Value { get; init; }
}

/// <summary>A single scalar metric with a severity classification (e.g. a Gauge/KpiCard value).</summary>
public sealed record WidgetDataMetricDto
{
    /// <summary>Metric value.</summary>
    public required double Value { get; init; }

    /// <summary>Metric label (e.g. "Compliance %").</summary>
    public required string Label { get; init; }

    /// <summary>Severity classification for the metric (e.g. "Critical", "High", "Medium", "Low", "None").</summary>
    public required string Severity { get; init; }
}

/// <summary>
/// Normalized widget data contract: <c>Series[]</c>, <c>Categories[]</c>, and <c>Metrics[]</c>,
/// per "3. Data Model for Widgets: Input: normalized series or tabular data".
/// </summary>
public sealed record WidgetDataContractDto
{
    /// <summary>Identifier of the widget this data contract was resolved for.</summary>
    public required Guid WidgetId { get; init; }

    /// <summary>UTC timestamp this data contract was resolved at.</summary>
    public required DateTime ResolvedAtUtc { get; init; }

    /// <summary>Time-series data, when applicable to the widget type.</summary>
    public required IReadOnlyList<WidgetDataSeriesDto> Series { get; init; }

    /// <summary>Categorical data, when applicable to the widget type.</summary>
    public required IReadOnlyList<WidgetDataCategoryDto> Categories { get; init; }

    /// <summary>Scalar metric data, when applicable to the widget type.</summary>
    public required IReadOnlyList<WidgetDataMetricDto> Metrics { get; init; }
}
