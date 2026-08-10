// ============================================================================
//  Project:        Shared.Authorization.Abstractions
//  Module:         Checks
//  File:           AuthorizationDecision.cs
//  Created:        2026-08-10
//  Description:    Phase 9 deliverable. AuthorizationCheck.Evaluate's return
//                  type. MfaRequirementFailed is broken out separately from
//                  IsAuthorized so AuthorizationTelemetry can increment
//                  PlantSafetyMetrics.MfaRequirementFailures specifically
//                  (rather than folding it into the generic
//                  AuthorizationDenials count) without re-deriving the
//                  reason from Reason's free-text message.
// ============================================================================
namespace Shared.Authorization.Abstractions.Checks;

/// <summary>The outcome of a single authorization check.</summary>
public sealed record AuthorizationDecision(bool IsAuthorized, string Reason, bool MfaRequirementFailed = false);
