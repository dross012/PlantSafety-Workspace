// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Domain
//  File:           PolicyFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Policy aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Policy.Factories;

/// <summary>Default factory implementation for constructing <see cref="Policy"/> aggregates.</summary>
public sealed class PolicyFactory : IPolicyFactory
{
    /// <inheritdoc/>
    public Policy Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => Policy.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
