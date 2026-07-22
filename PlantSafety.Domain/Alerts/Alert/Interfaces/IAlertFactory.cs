// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           IAlertFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing Alert aggregates.
//                  Additive-only evolution.
// ============================================================================
using AlertCategory = PlantSafety.Domain.Alerts.Alert.Enums.AlertCategory;
using AlertSeverity = PlantSafety.Domain.Alerts.Alert.Enums.AlertSeverity;
using AlertSourceType = PlantSafety.Domain.Alerts.Alert.Enums.AlertSourceType;
using AlertLocation = PlantSafety.Domain.Alerts.Alert.ValueObjects.AlertLocation;

namespace PlantSafety.Domain.Alerts.Alert.Interfaces;

/// <summary>Factory interface for constructing <see cref="Aggregates.Alert"/> aggregates.</summary>
public interface IAlertFactory
{
    /// <summary>Triggers a new Alert aggregate with the supplied parameters.</summary>
    Aggregates.Alert Trigger(
        UtcTimestamp triggeredAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        AlertCategory category,
        AlertSourceType sourceType,
        string title,
        AlertLocation? location = null,
        AlertSeverity? severity = null,
        UserId? createdBy = null);
}
