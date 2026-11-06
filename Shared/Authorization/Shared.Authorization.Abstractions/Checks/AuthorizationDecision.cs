// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Checks
//  File:           AuthorizationDecision.cs
//  Created:        2026-08-10
//  Description:    Phase 9 deliverable. The deterministic, pure-data result
//                  of an authorization check -- never thrown as an
//                  exception at the check site, always returned as a value
//                  so callers (and telemetry/governance) can observe both
//                  the allow and the deny path identically.
// ============================================================================
namespace Shared.Authorization.Abstractions.Checks;

/// <summary>The deterministic result of an authorization check.</summary>
public sealed record AuthorizationDecision(bool IsAuthorized, string Reason)
{
    /// <summary>Builds an authorized decision with the given reason.</summary>
    public static AuthorizationDecision Allow(string reason) => new(true, reason);

    /// <summary>Builds a denied decision with the given reason.</summary>
    public static AuthorizationDecision Deny(string reason) => new(false, reason);
}
