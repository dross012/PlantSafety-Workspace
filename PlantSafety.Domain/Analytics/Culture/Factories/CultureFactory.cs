// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Domain
//  File:           CultureFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Culture aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Culture.Factories;

/// <summary>Default factory implementation for constructing <see cref="Culture"/> aggregates.</summary>
public sealed class CultureFactory : ICultureFactory
{
    /// <inheritdoc/>
    public Culture Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => Culture.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
