// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           WidgetNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a Widget aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Dashboards.Widget.ValueObjects.Ids;

namespace PlantSafety.Domain.Dashboards.Widget.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.Widget"/> aggregate lookup finds no match.</summary>
public sealed class WidgetNotFoundException : WidgetDomainException
{
    /// <summary>Creates a not-found exception for the given Widget identity.</summary>
    public WidgetNotFoundException(WidgetId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"Widget with id '{id}' was not found.",
            "Widget.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
