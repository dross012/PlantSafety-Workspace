// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityNotification
//  Layer:          Domain
//  File:           CommunityNotificationInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CommunityNotification lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityNotification.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityNotification.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.CommunityNotification"/> lifecycle transition is not permitted.</summary>
public sealed class CommunityNotificationInvalidStatusTransitionException : CommunityNotificationDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public CommunityNotificationInvalidStatusTransitionException(
        CommunityNotificationId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition CommunityNotification '{id}' from '{from}' to '{to}'.",
            "CommunityNotification.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
