// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Domain
//  File:           IEngagementFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing Engagement aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Engagement.Interfaces;

/// <summary>Factory interface for constructing <see cref="Engagement"/> aggregates.</summary>
public interface IEngagementFactory
{
    /// <summary>Creates a new Engagement aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
