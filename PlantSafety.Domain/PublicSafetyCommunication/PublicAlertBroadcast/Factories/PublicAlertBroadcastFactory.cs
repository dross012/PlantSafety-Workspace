// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.PublicAlertBroadcast
//  Layer:          Domain
//  File:           PublicAlertBroadcastFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing PublicAlertBroadcast aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Factories;

/// <summary>Default factory implementation for constructing <see cref="PublicAlertBroadcast"/> aggregates.</summary>
public sealed class PublicAlertBroadcastFactory : IPublicAlertBroadcastFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates.PublicAlertBroadcast Create(
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
        string targetArea)
        => PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates.PublicAlertBroadcast.Create(
            createdBy, createdAtUtc, correlationId, causationId, actor, tenantId, siteId, incidentId, severity, channel, headline, message, targetArea);
}
