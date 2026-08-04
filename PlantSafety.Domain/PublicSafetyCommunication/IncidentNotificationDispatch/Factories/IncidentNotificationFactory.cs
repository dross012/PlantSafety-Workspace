// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.IncidentNotificationDispatch
//  Layer:          Domain
//  File:           IncidentNotificationFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing IncidentNotification aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Factories;

/// <summary>Default factory implementation for constructing <see cref="IncidentNotification"/> aggregates.</summary>
public sealed class IncidentNotificationFactory : IIncidentNotificationFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates.IncidentNotification Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        Guid? incidentId,
        PublicSafetyAgencyId publicSafetyAgencyId,
        PublicSafetyDeliveryChannel channel,
        string message)
        => PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates.IncidentNotification.Create(
            createdBy, createdAtUtc, correlationId, causationId, actor, tenantId, siteId, incidentId, publicSafetyAgencyId, channel, message);
}
