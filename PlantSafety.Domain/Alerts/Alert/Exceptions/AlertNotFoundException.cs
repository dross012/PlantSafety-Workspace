// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an Alert aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using AlertId = PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids.AlertId;

namespace PlantSafety.Domain.Alerts.Alert.Exceptions;

/// <summary>Thrown when an <see cref="Aggregates.Alert"/> aggregate lookup finds no match.</summary>
public sealed class AlertNotFoundException : AlertDomainException
{
    /// <summary>Creates a not-found exception for the given Alert identity.</summary>
    public AlertNotFoundException(AlertId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"Alert with id '{id}' was not found.",
            "Alert.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
