// ============================================================================
// File: WidgetsRequests.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: Request DTOs bound from the HTTP request body/query string for the
//              Dashboards V1 controller (Widget aggregate).
// ============================================================================

namespace PlantSafety.Api.Dashboards.Contracts.Requests;

/// <summary>Request body for creating a new draft Widget definition in the reusable widget library.</summary>
public sealed record CreateWidgetRequest
{
    /// <summary>Human-readable widget name.</summary>
    public required string Name { get; init; }

    /// <summary>Widget visualization type code (e.g. "LineChart", "Gauge").</summary>
    public required string TypeCode { get; init; }

    /// <summary>Owning module the widget's data source belongs to.</summary>
    public required string SourceModule { get; init; }

    /// <summary>Query/endpoint identifier within the owning module.</summary>
    public required string QueryOrEndpoint { get; init; }

    /// <summary>Optional explicit refresh cadence override, in seconds.</summary>
    public int? RefreshIntervalSeconds { get; init; }

    /// <summary>Configuration parameters (time range, plant, module, thresholds).</summary>
    public Dictionary<string, string>? Parameters { get; init; }

    /// <summary>Canonical role names permitted to view this widget.</summary>
    public IReadOnlyList<string>? PermittedRoles { get; init; }
}

/// <summary>Request body for reconfiguring a Widget's data source, refresh interval, and/or parameters.</summary>
public sealed record ConfigureWidgetRequest
{
    /// <summary>Optional new owning module for the widget's data source.</summary>
    public string? SourceModule { get; init; }

    /// <summary>Optional new query/endpoint identifier. Required when <see cref="SourceModule"/> is supplied.</summary>
    public string? QueryOrEndpoint { get; init; }

    /// <summary>Optional new refresh cadence, in seconds.</summary>
    public int? RefreshIntervalSeconds { get; init; }

    /// <summary>Optional replacement set of configuration parameters.</summary>
    public Dictionary<string, string>? Parameters { get; init; }
}

/// <summary>Request body for deprecating an active Widget.</summary>
public sealed record DeprecateWidgetRequest
{
    /// <summary>Reason for deprecation.</summary>
    public required string Reason { get; init; }
}

/// <summary>Query-string bound filter overrides for resolving a widget's data contract.</summary>
public sealed record GetWidgetDataRequest
{
    /// <summary>Optional filter overrides (time range, plant, line, department, shift) applied for this resolution.</summary>
    public Dictionary<string, string>? FilterOverrides { get; init; }
}
