// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Domain
//  File:           SafetyPolicyLifecycleRules.cs
//  Created:        2026-07-20
//  Updated:        2026-07-21 -- realigned with the real governance lifecycle
//                  (Draft -> PendingApproval -> Approved -> Published -> UnderReview -> Retired -> Archived).
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the SafetyPolicy aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyPolicies.Enums;

namespace PlantSafety.Domain.SafetyPolicies.Rules;

/// <summary>Defines permitted lifecycle transitions for the SafetyPolicy aggregate.</summary>
public static class SafetyPolicyLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the policy may be submitted for approval from <paramref name="current"/>.</summary>
    public static bool CanSubmitForApproval(SafetyPolicyStatus current) => current == SafetyPolicyStatus.Draft;

    /// <summary>Returns <see langword="true"/> when the policy may be approved from <paramref name="current"/>.</summary>
    public static bool CanApprove(SafetyPolicyStatus current) => current == SafetyPolicyStatus.PendingApproval;

    /// <summary>Returns <see langword="true"/> when the policy may be published from <paramref name="current"/>.</summary>
    public static bool CanPublish(SafetyPolicyStatus current) => current == SafetyPolicyStatus.Approved;

    /// <summary>Returns <see langword="true"/> when a stakeholder acknowledgment may be recorded from <paramref name="current"/>.</summary>
    public static bool CanAcknowledge(SafetyPolicyStatus current) => current == SafetyPolicyStatus.Published;

    /// <summary>Returns <see langword="true"/> when the mandated review cycle may start from <paramref name="current"/>.</summary>
    public static bool CanStartReview(SafetyPolicyStatus current) => current == SafetyPolicyStatus.Published;

    /// <summary>Returns <see langword="true"/> when the policy may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(SafetyPolicyStatus current)
        => current is SafetyPolicyStatus.Published or SafetyPolicyStatus.UnderReview;

    /// <summary>Returns <see langword="true"/> when the policy may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(SafetyPolicyStatus current) => current == SafetyPolicyStatus.Retired;
}
