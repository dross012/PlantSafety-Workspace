// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Domain
//  File:           SafetyPolicyInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a SafetyPolicy lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.SafetyPolicies.ValueObjects.Ids;

namespace PlantSafety.Domain.SafetyPolicies.Exceptions;

/// <summary>Thrown when a SafetyPolicy lifecycle transition is not permitted.</summary>
public sealed class SafetyPolicyInvalidStatusTransitionException : SafetyPolicyDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public SafetyPolicyInvalidStatusTransitionException(
        SafetyPolicyId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'SafetyPolicy' {id} from '{from}' to '{to}'.",
            "SafetyPolicy.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}