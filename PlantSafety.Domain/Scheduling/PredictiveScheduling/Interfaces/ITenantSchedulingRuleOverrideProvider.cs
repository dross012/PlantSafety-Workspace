// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           ITenantSchedulingRuleOverrideProvider.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §1.4 Tenant-Aware Scheduling)
//  SchemaVersion:  2026.1
//  Description:    Domain-facing abstraction letting a tenant customize predictive-scheduling
//                  constraint weights/thresholds, backed by real configuration (Infrastructure
//                  implementation), never per-tenant hardcoded branching logic. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Interfaces;

/// <summary>Resolves a tenant's constraint-weight/threshold overrides for the predictive scheduling rule engine.</summary>
public interface ITenantSchedulingRuleOverrideProvider
{
    /// <summary>
    /// Resolves the constraint-weight/threshold override set for <paramref name="tenantId"/>, or
    /// <see cref="TenantSchedulingRuleOverrides.Default"/> when the tenant has no overrides
    /// configured.
    /// </summary>
    TenantSchedulingRuleOverrides GetOverridesFor(Guid tenantId);
}

/// <summary>
/// A tenant's constraint-weight/threshold overrides for the predictive scheduling rule engine.
/// Every member is a multiplier/threshold applied on top of the platform-default rule pipeline
/// (<see cref="Services.ConstraintEvaluator.DefaultRules"/>); a value of <c>1.0</c> (weight
/// multipliers) or the platform default (thresholds) means "no override".
/// </summary>
/// <param name="SafetyCriticalPriorityMultiplier">Multiplier applied to <c>BasePriorityWeight</c> for tasks flagged <c>IsSafetyCritical</c>.</param>
/// <param name="OverduePriorityMultiplier">Multiplier applied to <c>BasePriorityWeight</c> for tasks flagged <c>IsOverdue</c>.</param>
/// <param name="MaxToleratedWarningViolations">Maximum Warning-severity constraint violations tolerated per accepted assignment before it is treated as infeasible for this tenant.</param>
public sealed record TenantSchedulingRuleOverrides(
    double SafetyCriticalPriorityMultiplier,
    double OverduePriorityMultiplier,
    int MaxToleratedWarningViolations)
{
    /// <summary>The platform-default override set (no customization) -- 1.5x safety-critical, 1.25x overdue, tolerate up to 3 Warning violations.</summary>
    public static readonly TenantSchedulingRuleOverrides Default = new(
        SafetyCriticalPriorityMultiplier: 1.5,
        OverduePriorityMultiplier: 1.25,
        MaxToleratedWarningViolations: 3);
}
