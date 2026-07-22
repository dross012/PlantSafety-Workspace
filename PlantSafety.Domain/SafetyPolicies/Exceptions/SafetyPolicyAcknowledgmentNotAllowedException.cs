// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Domain
//  File:           SafetyPolicyAcknowledgmentNotAllowedException.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when acknowledgment is attempted against a SafetyPolicy that is not
//                  currently Published (only the actively governing revision may be acknowledged).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.SafetyPolicies.ValueObjects.Ids;

namespace PlantSafety.Domain.SafetyPolicies.Exceptions;

/// <summary>Thrown when acknowledgment is attempted against a non-Published SafetyPolicy.</summary>
public sealed class SafetyPolicyAcknowledgmentNotAllowedException : SafetyPolicyDomainException
{
    /// <summary>Creates an acknowledgment-not-allowed exception for the given policy and status.</summary>
    public SafetyPolicyAcknowledgmentNotAllowedException(
        SafetyPolicyId id, string currentStatus,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot record acknowledgment for SafetyPolicy {id}: current status is '{currentStatus}', expected 'Published'.",
            "SafetyPolicy.AcknowledgmentNotAllowed",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
