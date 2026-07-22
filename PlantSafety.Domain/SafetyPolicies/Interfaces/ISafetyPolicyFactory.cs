// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Domain
//  File:           ISafetyPolicyFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing SafetyPolicy aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyPolicies.Aggregates;

namespace PlantSafety.Domain.SafetyPolicies.Interfaces;

/// <summary>Factory interface for constructing SafetyPolicy aggregates.</summary>
public interface ISafetyPolicyFactory
{
    /// <summary>Creates a new SafetyPolicy aggregate with the supplied parameters.</summary>
    SafetyPolicy Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}