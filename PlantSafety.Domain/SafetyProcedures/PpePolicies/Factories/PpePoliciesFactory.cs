// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.PpePolicies
//  Layer:          Domain
//  File:           PpePoliciesFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing PpePolicies aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.PpePolicies.Factories;

/// <summary>Default factory implementation for constructing <see cref="PpePolicies"/> aggregates.</summary>
public sealed class PpePoliciesFactory : IPpePoliciesFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.SafetyProcedures.PpePolicies.Aggregates.PpePolicies Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.SafetyProcedures.PpePolicies.Aggregates.PpePolicies.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}