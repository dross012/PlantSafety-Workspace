// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.CommunicationLog
//  Layer:          Domain
//  File:           CommunicationLogEntryFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing CommunicationLogEntry aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Factories;

/// <summary>Default factory implementation for constructing <see cref="CommunicationLogEntry"/> aggregates.</summary>
public sealed class CommunicationLogEntryFactory : ICommunicationLogEntryFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Aggregates.CommunicationLogEntry Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        Guid? incidentId,
        Guid? relatedIncidentNotificationId,
        Guid? relatedPublicAlertBroadcastId,
        CommunicationDirection direction,
        CommunicationMedium medium,
        string counterpartyName,
        string summary,
        UtcTimestamp occurredAtUtc)
        => PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Aggregates.CommunicationLogEntry.Create(
            createdBy, createdAtUtc, correlationId, causationId, actor, tenantId, siteId, incidentId,
            relatedIncidentNotificationId, relatedPublicAlertBroadcastId, direction, medium, counterpartyName, summary, occurredAtUtc);
}
