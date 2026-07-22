// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           WidgetType.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the reusable Widget Library types defined in "5. Widget
//                  Library (Reusable Components)" and "Appendix E — Dashboard Widget Library".
//                  Carries metadata (whether the widget is spatial, whether it supports
//                  real-time push updates) because widget type drives rendering-contract shape
//                  and caching/refresh behavior — satisfying the Enums Rule (2.7) requirement for
//                  Smart Enums when a type has business rules and metadata.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Dashboards.Widget.Enums;

/// <summary>Classifies a <see cref="Aggregates.Widget"/> into one of the reusable widget library types.</summary>
public sealed class WidgetType : IEquatable<WidgetType>
{
    /// <summary>Canonical string code for this widget type (e.g. "LineChart").</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>True when this widget type renders spatial/geographic data (heatmaps, facility maps).</summary>
    public bool IsSpatial { get; }

    /// <summary>True when this widget type is typically driven by real-time push updates rather than pre-aggregated data.</summary>
    public bool SupportsRealTimePush { get; }

    private WidgetType(string code, string displayName, bool isSpatial, bool supportsRealTimePush)
    {
        Code = code;
        DisplayName = displayName;
        IsSpatial = isSpatial;
        SupportsRealTimePush = supportsRealTimePush;
    }

    /// <summary>Trend visualization (TRIR, DART, incidents, near-misses, sensor data).</summary>
    public static readonly WidgetType LineChart = new("LineChart", "Line Chart", false, true);

    /// <summary>Distribution visualization (hazard categories, root causes, permit types).</summary>
    public static readonly WidgetType BarChart = new("BarChart", "Bar Chart", false, false);

    /// <summary>Composition visualization (injury types, PPE types, incident sources).</summary>
    public static readonly WidgetType PieChart = new("PieChart", "Pie Chart", false, false);

    /// <summary>Score visualization (compliance %, culture score, risk score, PM completion).</summary>
    public static readonly WidgetType Gauge = new("Gauge", "Gauge", false, true);

    /// <summary>List visualization (active permits, open corrective actions, alerts, inspections).</summary>
    public static readonly WidgetType Table = new("Table", "Table", false, true);

    /// <summary>Spatial intensity visualization (high-risk zones, traffic, incident locations).</summary>
    public static readonly WidgetType Heatmap = new("Heatmap", "Heatmap", true, true);

    /// <summary>Facility layout / zone status visualization.</summary>
    public static readonly WidgetType Map = new("Map", "Facility Map", true, true);

    /// <summary>Chronological event visualization (alerts, incidents, permits, drills).</summary>
    public static readonly WidgetType Timeline = new("Timeline", "Timeline", false, true);

    /// <summary>Multi-series trend visualization (sensor/environmental trend graphs).</summary>
    public static readonly WidgetType TrendGraph = new("TrendGraph", "Trend Graph", false, true);

    /// <summary>Single-metric summary card (e.g. compliance %).</summary>
    public static readonly WidgetType KpiCard = new("KpiCard", "KPI Card", false, false);

    /// <summary>All defined widget types, per Appendix E — Dashboard Widget Library.</summary>
    public static readonly IReadOnlyList<WidgetType> All = new[]
    {
        LineChart, BarChart, PieChart, Gauge, Table, Heatmap, Map, Timeline, TrendGraph, KpiCard
    };

    /// <summary>Default widget type assigned when none is specified.</summary>
    public static readonly WidgetType Default = KpiCard;

    /// <summary>Resolves a <see cref="WidgetType"/> from its canonical code. Throws when unknown.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="code"/> does not match a known type.</exception>
    public static WidgetType From(string code)
        => TryParse(code, out var type) ? type! : throw new ArgumentException($"Unknown WidgetType code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a <see cref="WidgetType"/> from its canonical code.</summary>
    public static bool TryParse(string? code, out WidgetType? type)
    {
        type = All.FirstOrDefault(t => string.Equals(t.Code, code, StringComparison.OrdinalIgnoreCase));
        return type is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> matches a known widget type.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(WidgetType? other) => other is not null && Code == other.Code;
    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as WidgetType);
    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.Ordinal);
    /// <inheritdoc/>
    public override string ToString() => Code;
}
