// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.IncidentNotificationDispatch
//  Layer:          Domain
//  File:           IIncidentNotificationFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing IncidentNotification aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Interfaces;

/// <summary>Factory interface for constructing <see cref="IncidentNotification"/> aggregates.</summary>
public interface IIncidentNotificationFactory
{
    /// <summary>Creates and dispatches a new IncidentNotification aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates.IncidentNotification Create(
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
        string message);
}
