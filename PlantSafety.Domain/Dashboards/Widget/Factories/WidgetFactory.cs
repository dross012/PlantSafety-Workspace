// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           WidgetFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Widget aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Dashboards.Widget.Enums;
using PlantSafety.Domain.Dashboards.Widget.Interfaces;
using PlantSafety.Domain.Dashboards.Widget.ValueObjects;

namespace PlantSafety.Domain.Dashboards.Widget.Factories;

/// <summary>Default factory implementation for constructing <see cref="Aggregates.Widget"/> aggregates.</summary>
public sealed class WidgetFactory : IWidgetFactory
{
    /// <inheritdoc/>
    public Aggregates.Widget Create(
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        string name,
        WidgetType type,
        WidgetDataSource dataSource,
        UserId createdBy,
        WidgetRefreshInterval? refreshInterval = null,
        IEnumerable<WidgetParameter>? parameters = null,
        IEnumerable<string>? permittedRoles = null)
        => Aggregates.Widget.Create(
            createdAtUtc, correlationId, causationId, actor, tenantId, name, type, dataSource, createdBy,
            refreshInterval, parameters, permittedRoles);
}
