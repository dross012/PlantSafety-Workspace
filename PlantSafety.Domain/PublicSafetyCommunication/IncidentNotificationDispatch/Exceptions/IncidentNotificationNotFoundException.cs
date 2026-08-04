// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.IncidentNotificationDispatch
//  Layer:          Domain
//  File:           IncidentNotificationNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when an IncidentNotification aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Exceptions;

/// <summary>Thrown when an <see cref="IncidentNotification"/> aggregate lookup finds no match.</summary>
public sealed class IncidentNotificationNotFoundException : IncidentNotificationDomainException
{
    /// <summary>Creates a not-found exception for the given IncidentNotification identity.</summary>
    public IncidentNotificationNotFoundException(IncidentNotificationId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "IncidentNotification with id '{id}' was not found.",
            "IncidentNotification.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
