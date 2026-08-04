// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.PublicAlertBroadcast
//  Layer:          Domain
//  File:           PublicAlertBroadcastInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PublicAlertBroadcast lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Exceptions;

/// <summary>Thrown when a <see cref="PublicAlertBroadcast"/> lifecycle transition is not permitted.</summary>
public sealed class PublicAlertBroadcastInvalidStatusTransitionException : PublicAlertBroadcastDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public PublicAlertBroadcastInvalidStatusTransitionException(
        PublicAlertBroadcastId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'PublicAlertBroadcast' {id} from '{from}' to '{to}'.",
            "PublicAlertBroadcast.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
