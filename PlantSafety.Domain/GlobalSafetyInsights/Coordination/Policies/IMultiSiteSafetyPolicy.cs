// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights.Coordination
//  Layer:          Domain
//  File:           IMultiSiteSafetyPolicy.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Real, deterministic domain policy governing multi-site safety coordination.
//                  Given a TenantId and a set of candidate SiteIds, decides (a) which sites are
//                  eligible to coordinate together, (b) whether a detected anomaly's severity
//                  crosses the auto-escalation threshold (vs. requiring a manual trigger), and (c)
//                  whether a given action type requires an approval chain (mirroring the Phase 5
//                  Workflow module's HumanApproval step concept) before it may execute. No
//                  per-tenant special-casing -- every rule is a fixed, testable function of its
//                  inputs. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Coordination.Enums;

namespace PlantSafety.Domain.GlobalSafetyInsights.Coordination.Policies;

/// <summary>
/// Deterministic domain policy governing which sites may coordinate on a cross-site action,
/// when a detected anomaly should auto-escalate rather than await a manual trigger, and when an
/// action type requires an approval chain before execution.
/// </summary>
public interface IMultiSiteSafetyPolicy
{
    /// <summary>
    /// Evaluates <paramref name="candidateSiteIds"/> for eligibility to coordinate on
    /// <paramref name="actionType"/> and determines whether an approval chain is required.
    /// </summary>
    /// <param name="tenantId">The tenant issuing the coordinated action. Every eligible site is implicitly scoped to this tenant.</param>
    /// <param name="candidateSiteIds">The sites proposed to participate in the action.</param>
    /// <param name="actionType">The kind of coordinated action being evaluated.</param>
    /// <param name="tenantExcludedSiteIds">
    /// Sites the tenant has explicitly excluded from cross-site coordination (e.g. sites under a
    /// separate compliance boundary). Empty when the tenant has configured no exclusions.
    /// </param>
    MultiSiteCoordinationDecision Evaluate(
        Guid tenantId,
        IReadOnlyList<Guid> candidateSiteIds,
        CrossSiteActionType actionType,
        IReadOnlyList<Guid>? tenantExcludedSiteIds = null);

    /// <summary>
    /// Returns <see langword="true"/> when a detected anomaly's statistical severity
    /// (<paramref name="deviationMagnitude"/> -- the same z-score/deviation-ratio magnitude carried
    /// by <c>DetectedAnomaly.DeviationMagnitude</c>) crosses the threshold that triggers automatic
    /// escalation, rather than waiting for a human to manually trigger the coordinated action.
    /// </summary>
    bool ShouldAutoEscalate(double deviationMagnitude);
}
