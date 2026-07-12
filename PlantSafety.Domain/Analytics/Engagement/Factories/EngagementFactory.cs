// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Domain
//  File:           EngagementFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Engagement aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Engagement.Factories;

/// <summary>Default factory implementation for constructing <see cref="Engagement"/> aggregates.</summary>
public sealed class EngagementFactory : IEngagementFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
