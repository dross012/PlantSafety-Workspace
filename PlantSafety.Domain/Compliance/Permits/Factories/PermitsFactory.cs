// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Domain
//  File:           PermitsFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Permits aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Permits.Factories;

/// <summary>Default factory implementation for constructing <see cref="Permits"/> aggregates.</summary>
public sealed class PermitsFactory : IPermitsFactory
{
    /// <inheritdoc/>
    public Permits Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => Permits.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
