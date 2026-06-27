// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Domain
//  File:           EngagementInvalidStatusTransitionException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Engagement.Exceptions;

/// <summary>Thrown when a <see cref="Engagement"/> lifecycle transition is not permitted.</summary>
public sealed class EngagementInvalidStatusTransitionException : EngagementDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public EngagementInvalidStatusTransitionException(
        EngagementId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'Engagement' {id} from '{from}' to '{to}'.",
            "Engagement.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
