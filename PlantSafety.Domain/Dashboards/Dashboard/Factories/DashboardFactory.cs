// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Dashboard aggregates.
//                  Additive-only evolution.
// ============================================================================
using IDashboardFactory = PlantSafety.Domain.Dashboards.Dashboard.Interfaces.IDashboardFactory;
using DashboardCategory = PlantSafety.Domain.Dashboards.Dashboard.Enums.DashboardCategory;
using DashboardFilter = PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.DashboardFilter;

namespace PlantSafety.Domain.Dashboards.Dashboard.Factories;

/// <summary>Default factory implementation for constructing <see cref="Aggregates.Dashboard"/> aggregates.</summary>
public sealed class DashboardFactory : IDashboardFactory
{
    /// <inheritdoc/>
    public Aggregates.Dashboard Create(
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string name,
        string description,
        DashboardCategory category,
        UserId createdBy,
        IEnumerable<DashboardFilter>? defaultFilters = null)
        => Aggregates.Dashboard.Create(
            createdAtUtc, correlationId, causationId, actor, tenantId, siteId, name, description, category, createdBy, defaultFilters);
}
