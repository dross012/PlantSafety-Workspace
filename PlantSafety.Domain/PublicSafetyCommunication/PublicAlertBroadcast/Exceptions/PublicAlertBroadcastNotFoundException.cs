// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.PublicAlertBroadcast
//  Layer:          Domain
//  File:           PublicAlertBroadcastNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PublicAlertBroadcast aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Exceptions;

/// <summary>Thrown when a <see cref="PublicAlertBroadcast"/> aggregate lookup finds no match.</summary>
public sealed class PublicAlertBroadcastNotFoundException : PublicAlertBroadcastDomainException
{
    /// <summary>Creates a not-found exception for the given PublicAlertBroadcast identity.</summary>
    public PublicAlertBroadcastNotFoundException(PublicAlertBroadcastId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "PublicAlertBroadcast with id '{id}' was not found.",
            "PublicAlertBroadcast.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
