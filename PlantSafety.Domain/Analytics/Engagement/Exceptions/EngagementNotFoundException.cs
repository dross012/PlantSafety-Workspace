// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Domain
//  File:           EngagementNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Engagement.Exceptions;

/// <summary>Thrown when a <see cref="Engagement"/> aggregate lookup finds no match.</summary>
public sealed class EngagementNotFoundException : EngagementDomainException
{
    /// <summary>Creates a not-found exception for the given Engagement identity.</summary>
    public EngagementNotFoundException(EngagementId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "Engagement with id '{id}' was not found.",
            "Engagement.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
