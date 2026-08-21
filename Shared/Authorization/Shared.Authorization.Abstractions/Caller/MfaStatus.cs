// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Caller
//  File:           MfaStatus.cs
//  Created:        2026-08-10
//  Description:    Phase 9 (Authorization Modernization) deliverable. The
//                  multi-factor-authentication state a CallerContext carries.
//                  Deliberately three states, not a bool: this program has no
//                  real authentication scheme or session model (confirmed
//                  standing fact since Phase 3.9), so "was MFA checked" and
//                  "did MFA pass" are different questions -- NotApplicable
//                  answers the first honestly (no real session exists to
//                  check) without being conflated with Verified (which would
//                  incorrectly imply a real MFA check actually ran and
//                  passed).
// ============================================================================
namespace Shared.Authorization.Abstractions.Caller;

/// <summary>The multi-factor-authentication state associated with a CallerContext.</summary>
public enum MfaStatus
{
    /// <summary>No real authentication/session model exists yet to check MFA against -- neither verified nor failed, simply not evaluable.</summary>
    NotApplicable,

    /// <summary>A real session's "amr" claim was checked and did not contain "mfa".</summary>
    NotVerified,

    /// <summary>A real session's "amr" claim was checked and contained "mfa".</summary>
    Verified,
}
