// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights.Coordination
//  Layer:          Domain
//  File:           SiteIneligibilityReason.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Simple enum (Handbook §2.7.B) explaining why a candidate SiteId was excluded
//                  from a coordinated action by IMultiSiteSafetyPolicy.Evaluate. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Domain.GlobalSafetyInsights.Coordination.Policies;

/// <summary>Reason a candidate site was excluded from a coordinated action's eligible set.</summary>
public enum SiteIneligibilityReason
{
    /// <summary>The candidate SiteId is <see cref="Guid.Empty"/> or otherwise structurally invalid.</summary>
    InvalidSiteId = 0,

    /// <summary>The tenant has explicitly excluded this site from cross-site coordination.</summary>
    ExcludedByTenantPolicy = 1,

    /// <summary>The site was a duplicate of an already-eligible candidate.</summary>
    Duplicate = 2,
}
