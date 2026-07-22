// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Domain
//  File:           SafetyPolicyStatus.cs
//  Created:        2026-07-20
//  Updated:        2026-07-21 -- replaced generic CRUD lifecycle with the real governance
//                  lifecycle documented in the Safety Policies Module (draft -> approval ->
//                  publication -> periodic review -> retirement).
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the SafetyPolicy aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyPolicies.Enums;

/// <summary>Lifecycle states for the SafetyPolicy aggregate root.</summary>
public enum SafetyPolicyStatus
{
    /// <summary>Policy content is being authored and has not been submitted for approval.</summary>
    Draft = 0,

    /// <summary>Policy has been submitted and awaits sign-off from the approval chain.</summary>
    PendingApproval = 1,

    /// <summary>Policy has been approved by the accountable authority but is not yet effective.</summary>
    Approved = 2,

    /// <summary>Policy is published, effective, and open for stakeholder acknowledgment.</summary>
    Published = 3,

    /// <summary>Policy's periodic review cycle has started; content may change before republication.</summary>
    UnderReview = 4,

    /// <summary>Policy has been superseded or withdrawn and no longer governs behavior.</summary>
    Retired = 5,

    /// <summary>Policy has been archived for long-term retention.</summary>
    Archived = 6,
}
