// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           IDashboardFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing Dashboard aggregates.
//                  Additive-only evolution.
// ============================================================================
using DashboardCategory = PlantSafety.Domain.Dashboards.Dashboard.Enums.DashboardCategory;
using DashboardFilter = PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.DashboardFilter;

namespace PlantSafety.Domain.Dashboards.Dashboard.Interfaces;

/// <summary>Factory interface for constructing <see cref="Aggregates.Dashboard"/> aggregates.</summary>
public interface IDashboardFactory
{
    /// <summary>Creates a new Dashboard aggregate with the supplied parameters.</summary>
    Aggregates.Dashboard Create(
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
        IEnumerable<DashboardFilter>? defaultFilters = null);
}
