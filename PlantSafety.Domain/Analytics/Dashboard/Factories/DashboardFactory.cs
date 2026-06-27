// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Domain
//  File:           DashboardFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Dashboard aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Dashboard.Factories;

/// <summary>Default factory implementation for constructing <see cref="Dashboard"/> aggregates.</summary>
public sealed class DashboardFactory : IDashboardFactory
{
    /// <inheritdoc/>
    public Dashboard Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => Dashboard.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
