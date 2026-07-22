// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           WidgetDataSource.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Value Object identifying the read-model query or endpoint a Widget pulls data
//                  from, per "2. Widget: Has: ... DataSource (query or endpoint)" and
//                  "1. Data Sources (Domain Modules): Each module exposes read models or query
//                  endpoints optimized for dashboard use."
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Dashboards.Widget.ValueObjects;

/// <summary>
/// Immutable value object identifying the owning module and the read-model query/endpoint a
/// <see cref="Aggregates.Widget"/> resolves its data from.
/// </summary>
public readonly record struct WidgetDataSource
{
    /// <summary>Canonical owning module name (e.g. "Alerts", "RiskManagement", "PPEManagement").</summary>
    public string SourceModule { get; }

    /// <summary>Canonical query/endpoint identifier within the owning module (e.g. "GetActiveAlertsQuery").</summary>
    public string QueryOrEndpoint { get; }

    private WidgetDataSource(string sourceModule, string queryOrEndpoint)
    {
        SourceModule = sourceModule;
        QueryOrEndpoint = queryOrEndpoint;
    }

    /// <summary>Creates a normalized <see cref="WidgetDataSource"/>. Rejects null/empty/whitespace components.</summary>
    /// <exception cref="ArgumentException">Thrown when either component is null, empty, or whitespace.</exception>
    public static WidgetDataSource Create(string sourceModule, string queryOrEndpoint)
    {
        if (string.IsNullOrWhiteSpace(sourceModule))
            throw new ArgumentException("WidgetDataSource.SourceModule must not be null or whitespace.", nameof(sourceModule));

        if (string.IsNullOrWhiteSpace(queryOrEndpoint))
            throw new ArgumentException("WidgetDataSource.QueryOrEndpoint must not be null or whitespace.", nameof(queryOrEndpoint));

        return new WidgetDataSource(sourceModule.Trim(), queryOrEndpoint.Trim());
    }

    /// <inheritdoc/>
    public override string ToString() => $"{SourceModule}/{QueryOrEndpoint}";
}
