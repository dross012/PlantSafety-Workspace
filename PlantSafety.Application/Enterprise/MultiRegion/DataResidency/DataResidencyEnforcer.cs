// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Application.Enterprise.MultiRegion.DataResidency
//  Layer:          Application
//  File:           DataResidencyEnforcer.cs
//  Created:        2026-08-03
//  Author:         Security & Compliance Agent (Phase 7 §5.3)
//  SchemaVersion:  2026.1
//  Description:    Default IDataResidencyEnforcer: compares a tenant's configured
//                  DataResidencyPolicy (ITenantDataResidencyProfileStore) against the platform's
//                  current deployment region (ICurrentDeploymentRegionProvider). Pure,
//                  deterministic, fully unit-testable without any database/network dependency.
//                  Additive-only evolution.
//
//                  2026-08-03 UPDATE (Phase 9 §4 Observability & Telemetry Expansion): each
//                  individual violation was already loudly logged by callers (e.g.
//                  MultiSiteActionRouter's catch block), but the enforcer itself emitted NOTHING --
//                  not even on the success path, by design (a passing check is not warning-worthy
//                  per Handbook §2.27 Canonical Log Volume Rule). That left a genuine monitoring
//                  gap: there was no way to tell whether residency enforcement was even running at
//                  all (vs. silently bypassed/never invoked) without grepping logs for the rare
//                  violation case. Adds a real, lightweight System.Diagnostics.Metrics counter
//                  (aggregate pass/fail totals, tagged by outcome) -- discovered automatically by
//                  TracingServiceRegistration's "PlantSafety.*" AddMeter wildcard, no new
//                  registration required. Deliberately NOT a per-check log line (would violate the
//                  Anti-Noise rule on this near-every-request hot path) and NOT a new SQL metrics
//                  table (a counter is the proportionate fix for "is this even running").
//                  Additive-only evolution.
// ============================================================================
using System.Diagnostics.Metrics;
using PlantSafety.Domain.MultiRegion.DataResidency;
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Application.Enterprise.MultiRegion.DataResidency;

/// <summary>Default <see cref="IDataResidencyEnforcer"/> implementation.</summary>
public sealed class DataResidencyEnforcer(
    ITenantDataResidencyProfileStore profileStore,
    ICurrentDeploymentRegionProvider currentDeploymentRegionProvider) : IDataResidencyEnforcer
{
    private static readonly Meter Meter = new("PlantSafety.MultiRegion.DataResidency");

    /// <summary>
    /// Aggregate count of data-residency checks performed, tagged by <c>result</c>
    /// ("satisfied"/"violation") -- the queryable answer to "is residency enforcement running at
    /// all" that per-incident violation logging alone cannot provide.
    /// </summary>
    private static readonly Counter<long> ChecksCounter =
        Meter.CreateCounter<long>("plantsafety.data_residency.checks_total", unit: "{check}",
            description: "Count of data-residency checks performed, tagged by result (satisfied/violation).");

    /// <inheritdoc/>
    public bool IsSatisfied(Guid tenantId)
    {
        var profile = profileStore.GetProfile(tenantId);
        var currentRegion = currentDeploymentRegionProvider.CurrentRegion;

        var satisfied = profile.Policy.IsSatisfiedByZone(currentRegion.DataResidencyZone);
        ChecksCounter.Add(1, new KeyValuePair<string, object?>("result", satisfied ? "satisfied" : "violation"));
        return satisfied;
    }

    /// <inheritdoc/>
    public void EnsureSatisfied(Guid tenantId, CorrelationId correlationId, CausationId causationId)
    {
        var profile = profileStore.GetProfile(tenantId);
        var currentRegion = currentDeploymentRegionProvider.CurrentRegion;

        var satisfied = profile.Policy.IsSatisfiedByZone(currentRegion.DataResidencyZone);
        ChecksCounter.Add(1, new KeyValuePair<string, object?>("result", satisfied ? "satisfied" : "violation"));

        if (!satisfied)
        {
            throw new DataResidencyViolationException(tenantId, profile.Policy, currentRegion, correlationId, causationId);
        }
    }
}
