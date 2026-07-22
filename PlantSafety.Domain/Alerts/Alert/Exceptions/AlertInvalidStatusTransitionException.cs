// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an Alert lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using AlertId = PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids.AlertId;

namespace PlantSafety.Domain.Alerts.Alert.Exceptions;

/// <summary>Thrown when an <see cref="Aggregates.Alert"/> lifecycle transition is not permitted.</summary>
public sealed class AlertInvalidStatusTransitionException : AlertDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public AlertInvalidStatusTransitionException(
        AlertId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition Alert '{id}' from '{from}' to '{to}'.",
            "Alert.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
