// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a Dashboard aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using DashboardId = PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids.DashboardId;

namespace PlantSafety.Domain.Dashboards.Dashboard.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.Dashboard"/> aggregate lookup finds no match.</summary>
public sealed class DashboardNotFoundException : DashboardDomainException
{
    /// <summary>Creates a not-found exception for the given Dashboard identity.</summary>
    public DashboardNotFoundException(DashboardId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"Dashboard with id '{id}' was not found.",
            "Dashboard.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
