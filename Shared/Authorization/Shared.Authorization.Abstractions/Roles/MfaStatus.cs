// ============================================================================
//  Project:        Shared.Authorization.Abstractions
//  Module:         Roles
//  File:           MfaStatus.cs
//  Created:        2026-08-10
//  Description:    Phase 9 deliverable. A CallerContext's multi-factor
//                  authentication status. AuthorizationCheck.Evaluate
//                  (Checks/) requires MfaStatus.Verified for
//                  ModulePermissionVerb.Admin -- the one MFA-gated tier --
//                  and PlantSafetyMetrics.MfaRequirementFailures (Phase 8/9)
//                  counts denials caused by that specific gate.
// ============================================================================
namespace Shared.Authorization.Abstractions.Roles;

/// <summary>A caller's multi-factor authentication status.</summary>
public enum MfaStatus
{
    /// <summary>MFA is not required for this caller/context (e.g. the system-default caller, or a non-Admin operation).</summary>
    NotApplicable,

    /// <summary>MFA is required but has not yet been verified.</summary>
    Required,

    /// <summary>MFA has been verified for this caller.</summary>
    Verified,
}
