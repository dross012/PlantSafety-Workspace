// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           IWidgetFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing Widget aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Dashboards.Widget.Enums;
using PlantSafety.Domain.Dashboards.Widget.ValueObjects;

namespace PlantSafety.Domain.Dashboards.Widget.Interfaces;

/// <summary>Factory interface for constructing <see cref="Aggregates.Widget"/> aggregates.</summary>
public interface IWidgetFactory
{
    /// <summary>Creates a new Widget aggregate with the supplied parameters.</summary>
    Aggregates.Widget Create(
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
        IEnumerable<string>? permittedRoles = null);
}
