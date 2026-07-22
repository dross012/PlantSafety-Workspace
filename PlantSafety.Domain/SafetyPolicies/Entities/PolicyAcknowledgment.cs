// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Domain
//  File:           PolicyAcknowledgment.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Child entity recording a single stakeholder acknowledgment of a Published
//                  SafetyPolicy revision. Grounded in "acknowledgment tracking" / "training
//                  documentation and retention" requirements of the Safety Policies Module.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyPolicies.ValueObjects.Ids;

namespace PlantSafety.Domain.SafetyPolicies.Entities;

/// <summary>Records that a specific user acknowledged a specific SafetyPolicy version.</summary>
public sealed class PolicyAcknowledgment
{
    /// <summary>Unique identity of this acknowledgment record.</summary>
    public Guid AcknowledgmentId { get; private set; }

    /// <summary>Identity of the parent SafetyPolicy aggregate.</summary>
    public SafetyPolicyId SafetyPolicyId { get; private set; }

    /// <summary>Policy version acknowledged, captured at time of acknowledgment.</summary>
    public int AcknowledgedPolicyVersion { get; private set; }

    /// <summary>Identity of the user who acknowledged the policy.</summary>
    public UserId UserId { get; private set; }

    /// <summary>UTC instant of the acknowledgment.</summary>
    public UtcTimestamp AcknowledgedAtUtc { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private PolicyAcknowledgment() { }

    /// <summary>Creates a new acknowledgment record.</summary>
    public PolicyAcknowledgment(
        Guid acknowledgmentId,
        SafetyPolicyId safetyPolicyId,
        int acknowledgedPolicyVersion,
        UserId userId,
        UtcTimestamp acknowledgedAtUtc)
    {
        AcknowledgmentId = acknowledgmentId;
        SafetyPolicyId = safetyPolicyId;
        AcknowledgedPolicyVersion = acknowledgedPolicyVersion;
        UserId = userId;
        AcknowledgedAtUtc = acknowledgedAtUtc;
    }
}
