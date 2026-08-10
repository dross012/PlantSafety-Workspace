// ============================================================================
//  Project:        Shared.Authorization.Abstractions
//  Module:         Caller
//  File:           CallerContext.cs
//  Created:        2026-08-10
//  Description:    Phase 9 deliverable. The resolved identity every
//                  authorization/tenant check in this program takes as
//                  input. SystemDefault (EnterpriseRole.PlatformAdmin,
//                  MfaStatus.NotApplicable) is the honest, documented
//                  placeholder ApiKeyAuthenticationMiddleware/
//                  CallerContextFactory fall back to when no API key is
//                  configured or the presented key is unrecognized -- not a
//                  route around authorization: it reproduces this program's
//                  pre-Phase-10 behavior (every request already ran as an
//                  implicit full-trust caller) exactly, now made explicit
//                  and observable instead of silently assumed. See
//                  WorkflowEngineController.cs's own header comment for the
//                  live call site this reasoning was first documented at.
// ============================================================================
using Shared.Authorization.Abstractions.Roles;

namespace Shared.Authorization.Abstractions.Caller;

/// <summary>The resolved identity every authorization/tenant check takes as input.</summary>
public sealed record CallerContext(EnterpriseRole Role, MfaStatus MfaStatus, string CorrelationId)
{
    /// <summary>
    /// The honest, documented placeholder caller used when no API key is configured, no key is
    /// presented, or the presented key is unrecognized. EnterpriseRole.PlatformAdmin, MfaStatus.
    /// NotApplicable -- reproduces this program's pre-Phase-10 implicit full-trust behavior exactly.
    /// </summary>
    public static CallerContext SystemDefault(string correlationId) => new(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, correlationId);
}
