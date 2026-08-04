// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.CommunicationLog
//  Layer:          Domain
//  File:           ICommunicationLogEntryFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing CommunicationLogEntry aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Interfaces;

/// <summary>Factory interface for constructing <see cref="CommunicationLogEntry"/> aggregates.</summary>
public interface ICommunicationLogEntryFactory
{
    /// <summary>Records a new CommunicationLogEntry aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Aggregates.CommunicationLogEntry Create(
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
        UtcTimestamp occurredAtUtc);
}
