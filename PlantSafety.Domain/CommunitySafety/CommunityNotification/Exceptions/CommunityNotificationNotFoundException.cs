// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityNotification
//  Layer:          Domain
//  File:           CommunityNotificationNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CommunityNotification aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityNotification.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityNotification.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.CommunityNotification"/> aggregate lookup finds no match.</summary>
public sealed class CommunityNotificationNotFoundException : CommunityNotificationDomainException
{
    /// <summary>Creates a not-found exception for the given CommunityNotification identity.</summary>
    public CommunityNotificationNotFoundException(CommunityNotificationId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"CommunityNotification with id '{id}' was not found.",
            "CommunityNotification.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
