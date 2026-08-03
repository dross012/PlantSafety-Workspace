// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights.Coordination
//  Layer:          Application
//  File:           CrossSiteLockoutPropagationCommand.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Would dispatch a real lockout/tagout propagation to each affected site's real
//                  Operational.LockoutTagout command surface. HONESTY NOTE (verified before
//                  writing this command): as of this pass, Operational.LockoutTagout has NO write
//                  command surface at all -- PlantSafety.Application.Operational.LockoutTagout
//                  contains only GetLockoutTagoutQuery/GetLockoutTagoutQueryHandler and a read
//                  repository; there is no LockoutTagout aggregate under PlantSafety.Domain, no
//                  Create/Apply/Release command, and no write repository to dispatch to. Rather
//                  than fabricate a fake integration against a module that cannot support one, this
//                  command's handler honestly reports the gap (Result.Failure with a clear,
//                  diagnosable error code) so callers/dashboards see an accurate "not yet
//                  integrable" status instead of a false success. Once Operational.LockoutTagout
//                  grows a real write-side aggregate/command (Create/Apply/Release), this handler
//                  is the intended integration point -- see its remarks for the exact dispatch shape
//                  to wire in at that time. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.GlobalSafetyInsights.Coordination.Commands;

/// <summary>Propagates a lockout/tagout action across a set of affected sites, as part of a governed cross-site coordinated action.</summary>
/// <param name="TenantId">Tenant identifier.</param>
/// <param name="AffectedSiteIds">Sites the lockout propagation was requested for (already policy-eligible; see MultiSiteActionRouter).</param>
/// <param name="Reason">Human-readable reason for the lockout propagation.</param>
/// <param name="InitiatedByUserId">User that initiated the coordinated action, if human-triggered; <see langword="null"/> for a system-triggered (auto-escalated) action.</param>
/// <param name="CorrelationId">Correlation identifier propagated from the triggering anomaly/request.</param>
/// <param name="CausationId">Causation identifier propagated from the triggering anomaly/request.</param>
public sealed record CrossSiteLockoutPropagationCommand(
    Guid TenantId,
    IReadOnlyList<Guid> AffectedSiteIds,
    string Reason,
    Guid? InitiatedByUserId,
    Guid CorrelationId,
    Guid CausationId) : IRequest<Result<CrossSiteLockoutPropagationResult>>;

/// <summary>Result of a <see cref="CrossSiteLockoutPropagationCommand"/> execution.</summary>
/// <param name="AttemptedSiteIds">Sites the propagation was attempted for.</param>
/// <param name="ModuleIntegrationAvailable">
/// Always <see langword="false"/> today -- Operational.LockoutTagout has no write command surface
/// to dispatch to (see file remarks). Kept as an explicit field (rather than only a Result.Failure)
/// so dashboards/tests can distinguish "genuinely too thin to integrate" from a transient error.
/// </param>
public sealed record CrossSiteLockoutPropagationResult(IReadOnlyList<Guid> AttemptedSiteIds, bool ModuleIntegrationAvailable);
