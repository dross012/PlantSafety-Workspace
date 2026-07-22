// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Domain
//  File:           SafetyPolicyFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing SafetyPolicy aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyPolicies.Aggregates;
using PlantSafety.Domain.SafetyPolicies.Interfaces;

namespace PlantSafety.Domain.SafetyPolicies.Factories;

/// <summary>Default factory implementation for constructing SafetyPolicy aggregates.</summary>
public sealed class SafetyPolicyFactory : ISafetyPolicyFactory
{
    /// <inheritdoc/>
    public SafetyPolicy Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => SafetyPolicy.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}