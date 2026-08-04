// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.PublicAlertBroadcast
//  Layer:          Domain
//  File:           IPublicAlertBroadcastFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing PublicAlertBroadcast aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Interfaces;

/// <summary>Factory interface for constructing <see cref="PublicAlertBroadcast"/> aggregates.</summary>
public interface IPublicAlertBroadcastFactory
{
    /// <summary>Creates and sends a new PublicAlertBroadcast aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates.PublicAlertBroadcast Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        Guid? incidentId,
        PublicAlertSeverity severity,
        PublicBroadcastChannel channel,
        string headline,
        string message,
        string targetArea);
}
