// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a Dashboard lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using DashboardId = PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids.DashboardId;

namespace PlantSafety.Domain.Dashboards.Dashboard.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.Dashboard"/> lifecycle transition is not permitted.</summary>
public sealed class DashboardInvalidStatusTransitionException : DashboardDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public DashboardInvalidStatusTransitionException(
        DashboardId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition Dashboard '{id}' from '{from}' to '{to}'.",
            "Dashboard.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
