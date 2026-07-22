// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           WidgetInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a Widget lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Dashboards.Widget.ValueObjects.Ids;

namespace PlantSafety.Domain.Dashboards.Widget.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.Widget"/> lifecycle transition is not permitted.</summary>
public sealed class WidgetInvalidStatusTransitionException : WidgetDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public WidgetInvalidStatusTransitionException(
        WidgetId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition Widget '{id}' from '{from}' to '{to}'.",
            "Widget.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
