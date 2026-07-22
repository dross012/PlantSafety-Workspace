// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Alert aggregates.
//                  Additive-only evolution.
// ============================================================================
using IAlertFactory = PlantSafety.Domain.Alerts.Alert.Interfaces.IAlertFactory;
using AlertCategory = PlantSafety.Domain.Alerts.Alert.Enums.AlertCategory;
using AlertSeverity = PlantSafety.Domain.Alerts.Alert.Enums.AlertSeverity;
using AlertSourceType = PlantSafety.Domain.Alerts.Alert.Enums.AlertSourceType;
using AlertLocation = PlantSafety.Domain.Alerts.Alert.ValueObjects.AlertLocation;

namespace PlantSafety.Domain.Alerts.Alert.Factories;

/// <summary>Default factory implementation for constructing <see cref="Aggregates.Alert"/> aggregates.</summary>
public sealed class AlertFactory : IAlertFactory
{
    /// <inheritdoc/>
    public Aggregates.Alert Trigger(
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
        UserId? createdBy = null)
        => Aggregates.Alert.Trigger(
            triggeredAtUtc, correlationId, causationId, actor,
            tenantId, siteId, category, sourceType, title, location, severity, createdBy);
}
