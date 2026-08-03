// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights.Coordination
//  Layer:          Domain
//  File:           MultiSiteSafetyPolicy.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Real, deterministic implementation of IMultiSiteSafetyPolicy. Eligibility rules:
//                  a candidate site is eligible when it is a non-empty, non-duplicate GUID not
//                  present in the tenant's explicit exclusion list (region/residency eligibility is
//                  already enforced upstream by MultiSiteActionRouter via
//                  ITenantRegionRouter/IDataResidencyEnforcer at the whole-tenant level -- see
//                  remarks -- so this policy operates purely on the site-exclusion dimension).
//                  Escalation rule: a detected anomaly auto-escalates once its deviation magnitude
//                  crosses AutoEscalationDeviationMagnitudeThreshold; below that, the coordinated
//                  action must be manually triggered. Approval-chain rule: CorrectiveActionRollout,
//                  LockoutPropagation, and MaintenanceCoordination always require approval
//                  (they create durable operational obligations -- CAPA records, lockout state,
//                  work orders -- at sites outside the triggering site); IncidentEscalation requires
//                  approval only once its blast radius exceeds
//                  ApprovalRequiredSiteCountThreshold sites. No per-tenant special-casing.
//                  Additive-only evolution.
//
//                  REGION NOTE: the platform currently resolves exactly one Region per tenant
//                  (ITenantRegionRouter.ResolveRegion(Guid tenantId) — see Phase 7 §4.3), so every
//                  site belonging to a tenant necessarily shares that tenant's region today. A true
//                  "same region unless cross-residency-permitted" site-level check therefore has no
//                  discriminating power until a first-class Site aggregate with its own region
//                  affinity lands; MultiSiteActionRouter still performs the tenant-level
//                  region/residency check (Phase 7 pattern) before this policy ever runs, so no
//                  cross-region leakage is possible under the current architecture.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Coordination.Enums;

namespace PlantSafety.Domain.GlobalSafetyInsights.Coordination.Policies;

/// <summary>Deterministic, testable implementation of <see cref="IMultiSiteSafetyPolicy"/>.</summary>
public sealed class MultiSiteSafetyPolicy : IMultiSiteSafetyPolicy
{
    /// <summary>
    /// Minimum DetectedAnomaly.DeviationMagnitude (z-score/deviation-ratio) that triggers automatic
    /// escalation without a human manually triggering the coordinated action.
    /// </summary>
    public const double AutoEscalationDeviationMagnitudeThreshold = 3.0;

    /// <summary>
    /// Number of eligible sites above which an IncidentEscalation action requires an approval
    /// chain before execution (its blast radius is broad enough to warrant sign-off).
    /// </summary>
    public const int ApprovalRequiredSiteCountThreshold = 3;

    /// <inheritdoc/>
    public MultiSiteCoordinationDecision Evaluate(
        Guid tenantId,
        IReadOnlyList<Guid> candidateSiteIds,
        CrossSiteActionType actionType,
        IReadOnlyList<Guid>? tenantExcludedSiteIds = null)
    {
        ArgumentNullException.ThrowIfNull(candidateSiteIds);

        var excluded = tenantExcludedSiteIds is { Count: > 0 }
            ? new HashSet<Guid>(tenantExcludedSiteIds)
            : [];

        var eligible = new List<Guid>();
        var ineligible = new List<IneligibleSite>();
        var seen = new HashSet<Guid>();

        foreach (var siteId in candidateSiteIds)
        {
            if (siteId == Guid.Empty)
            {
                ineligible.Add(new IneligibleSite(siteId, SiteIneligibilityReason.InvalidSiteId));
                continue;
            }

            if (!seen.Add(siteId))
            {
                ineligible.Add(new IneligibleSite(siteId, SiteIneligibilityReason.Duplicate));
                continue;
            }

            if (excluded.Contains(siteId))
            {
                ineligible.Add(new IneligibleSite(siteId, SiteIneligibilityReason.ExcludedByTenantPolicy));
                continue;
            }

            eligible.Add(siteId);
        }

        var (requiresApproval, approvalReason) = DetermineApproval(actionType, eligible.Count);

        return new MultiSiteCoordinationDecision
        {
            EligibleSiteIds = eligible,
            IneligibleSites = ineligible,
            RequiresApproval = requiresApproval,
            ApprovalReason = approvalReason,
        };
    }

    /// <inheritdoc/>
    public bool ShouldAutoEscalate(double deviationMagnitude)
        => deviationMagnitude >= AutoEscalationDeviationMagnitudeThreshold;

    private static (bool RequiresApproval, string? Reason) DetermineApproval(CrossSiteActionType actionType, int eligibleSiteCount)
    {
        switch (actionType)
        {
            case CrossSiteActionType.CorrectiveActionRollout:
                return (true, "CorrectiveActionRollout creates durable CAPA obligations at every affected site and always requires approval.");

            case CrossSiteActionType.LockoutPropagation:
                return (true, "LockoutPropagation changes physical/operational safety state at every affected site and always requires approval.");

            case CrossSiteActionType.MaintenanceCoordination:
                return (true, "MaintenanceCoordination creates WorkOrder obligations at every affected site and always requires approval.");

            case CrossSiteActionType.IncidentEscalation when eligibleSiteCount > ApprovalRequiredSiteCountThreshold:
                return (true, $"IncidentEscalation spans {eligibleSiteCount} sites, exceeding the {ApprovalRequiredSiteCountThreshold}-site auto-approve threshold.");

            case CrossSiteActionType.IncidentEscalation:
                return (false, null);

            default:
                return (false, null);
        }
    }
}
