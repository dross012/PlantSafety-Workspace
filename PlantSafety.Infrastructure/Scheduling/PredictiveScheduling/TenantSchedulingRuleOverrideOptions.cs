// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Infrastructure
//  File:           TenantSchedulingRuleOverrideOptions.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §1.4 Tenant-Aware Scheduling)
//  SchemaVersion:  2026.1
//  Description:    Real, configuration-backed per-tenant constraint-weight/threshold override
//                  set for the predictive scheduling rule engine (Phase 7 §1.4). Bound from
//                  configuration section "Scheduling:TenantRuleOverrides" -- a dictionary keyed
//                  by TenantId (string) -- NOT per-tenant hardcoded if/switch branching. Additive-
//                  only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Scheduling.PredictiveScheduling;

/// <summary>Configuration-bound per-tenant predictive-scheduling rule overrides.</summary>
public sealed class TenantSchedulingRuleOverrideOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "Scheduling:TenantRuleOverrides";

    /// <summary>Per-tenant override entries, keyed by TenantId (string form).</summary>
    public Dictionary<string, TenantSchedulingRuleOverrideEntry> Tenants { get; init; } = [];
}

/// <summary>A single tenant's configured override entry.</summary>
public sealed class TenantSchedulingRuleOverrideEntry
{
    /// <summary>Multiplier applied to the safety-critical priority bonus. Defaults to the platform default (1.5) when unset.</summary>
    public double? SafetyCriticalPriorityMultiplier { get; init; }

    /// <summary>Multiplier applied to the overdue priority bonus. Defaults to the platform default (1.25) when unset.</summary>
    public double? OverduePriorityMultiplier { get; init; }

    /// <summary>Maximum tolerated Warning-severity constraint violations per accepted assignment. Defaults to the platform default (3) when unset.</summary>
    public int? MaxToleratedWarningViolations { get; init; }
}
