// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights.Coordination
//  Layer:          Domain
//  File:           MultiSiteCoordinationDecision.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Value object carrying the full, deterministic outcome of
//                  IMultiSiteSafetyPolicy.Evaluate: which candidate sites are eligible to
//                  coordinate together, which were excluded and why, and whether the action
//                  requires an approval chain before it may execute. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.GlobalSafetyInsights.Coordination.Policies;

/// <summary>A single candidate site excluded from a coordinated action's eligible set, with its reason.</summary>
/// <param name="SiteId">The excluded site.</param>
/// <param name="Reason">Why the site was excluded.</param>
public sealed record IneligibleSite(Guid SiteId, SiteIneligibilityReason Reason);

/// <summary>
/// The deterministic outcome of evaluating a coordinated action's candidate site set against
/// <see cref="IMultiSiteSafetyPolicy"/>.
/// </summary>
public sealed record MultiSiteCoordinationDecision
{
    /// <summary>Candidate sites eligible to participate in the coordinated action, in stable order.</summary>
    public required IReadOnlyList<Guid> EligibleSiteIds { get; init; }

    /// <summary>Candidate sites excluded from the coordinated action, with the reason for each exclusion.</summary>
    public required IReadOnlyList<IneligibleSite> IneligibleSites { get; init; }

    /// <summary>
    /// <see langword="true"/> when this action must be routed through an approval chain
    /// (mirroring the Phase 5 Workflow module's HumanApproval step concept) before it may execute
    /// against any eligible site.
    /// </summary>
    public required bool RequiresApproval { get; init; }

    /// <summary>Human-readable reason approval is required, or <see langword="null"/> when <see cref="RequiresApproval"/> is <see langword="false"/>.</summary>
    public string? ApprovalReason { get; init; }

    /// <summary><see langword="true"/> when at least one candidate site is eligible to coordinate.</summary>
    public bool IsEligible => EligibleSiteIds.Count > 0;
}
