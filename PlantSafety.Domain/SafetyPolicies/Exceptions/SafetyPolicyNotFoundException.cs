// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Domain
//  File:           SafetyPolicyNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a SafetyPolicy aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.SafetyPolicies.ValueObjects.Ids;

namespace PlantSafety.Domain.SafetyPolicies.Exceptions;

/// <summary>Thrown when a SafetyPolicy aggregate lookup finds no match.</summary>
public sealed class SafetyPolicyNotFoundException : SafetyPolicyDomainException
{
    /// <summary>Creates a not-found exception for the given SafetyPolicy identity.</summary>
    public SafetyPolicyNotFoundException(SafetyPolicyId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"SafetyPolicy with id '{id}' was not found.",
            "SafetyPolicy.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}