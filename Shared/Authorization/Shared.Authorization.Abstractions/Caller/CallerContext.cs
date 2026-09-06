// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Caller
//  File:           CallerContext.cs
//  Created:        2026-08-10
//  Description:    Phase 9 (Authorization Modernization) deliverable. The
//                  domain-level identity surrogate this program's
//                  authorization checks run against. PlantSafety.UI.Api has
//                  no authentication scheme, no tokens, no session state, and
//                  no identity provider (confirmed standing fact since Phase
//                  3.9) -- enabling ASP.NET Core's [Authorize]/
//                  UseAuthorization() would immediately break every existing,
//                  live-verified endpoint (Facility/Employee/Machine/
//                  WorkflowEngine) with no principal to authenticate against.
//                  CallerContext is not authenticated identity -- it is a
//                  deterministic, explicit, pure-data stand-in that lets the
//                  real Shared.Authorization role/permission/MFA model be
//                  exercised and made observable (permission checks actually
//                  run, telemetry actually fires) today, without inventing an
//                  authentication scheme this phase was never asked to build.
//
//                  SystemDefault() grants EnterpriseRole.PlatformAdmin (the
//                  highest tier) -- not because "the system" is a real,
//                  privileged actor, but because there is no real identity to
//                  grant a lower tier to and defaulting to Viewer would
//                  silently start rejecting every existing, working request
//                  the moment authorization checks are wired in. This is
//                  documented here, not hidden: PlatformAdmin-by-default is
//                  the honest placeholder until a real identity provider
//                  supplies a real CallerContext.
// ============================================================================
using Shared.Authorization.Abstractions.Roles;

namespace Shared.Authorization.Abstractions.Caller;

/// <summary>The deterministic, pure-data caller identity every authorization check runs against.</summary>
public sealed record CallerContext(
    EnterpriseRole Role,
    MfaStatus MfaStatus,
    string CorrelationId)
{
    /// <summary>
    /// The placeholder caller used everywhere no real identity provider exists yet (today,
    /// everywhere in this program). Grants EnterpriseRole.PlatformAdmin so that wiring
    /// authorization checks into existing call sites does not change their observable
    /// behavior -- see this file's own header comment for why.
    /// </summary>
    public static CallerContext SystemDefault(string correlationId) =>
        new(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, correlationId);
}
