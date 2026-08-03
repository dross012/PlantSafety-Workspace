// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Infrastructure
//  File:           TenantSchedulingRuleOverrideProvider.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §1.4 Tenant-Aware Scheduling)
//  SchemaVersion:  2026.1
//  Description:    ITenantSchedulingRuleOverrideProvider implementation backed by
//                  IOptionsMonitor<TenantSchedulingRuleOverrideOptions> (real configuration --
//                  appsettings/Key Vault/App Configuration, whichever the host wires in -- not
//                  per-tenant hardcoded logic). Falls back to
//                  TenantSchedulingRuleOverrides.Default for any tenant with no configured entry.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.Options;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Interfaces;

namespace PlantSafety.Infrastructure.Scheduling.PredictiveScheduling;

/// <summary>Configuration-backed <see cref="ITenantSchedulingRuleOverrideProvider"/>.</summary>
public sealed class TenantSchedulingRuleOverrideProvider(IOptionsMonitor<TenantSchedulingRuleOverrideOptions> options)
    : ITenantSchedulingRuleOverrideProvider
{
    /// <inheritdoc/>
    public TenantSchedulingRuleOverrides GetOverridesFor(Guid tenantId)
    {
        var configured = options.CurrentValue.Tenants;
        if (!configured.TryGetValue(tenantId.ToString(), out var entry))
        {
            return TenantSchedulingRuleOverrides.Default;
        }

        return new TenantSchedulingRuleOverrides(
            SafetyCriticalPriorityMultiplier: entry.SafetyCriticalPriorityMultiplier ?? TenantSchedulingRuleOverrides.Default.SafetyCriticalPriorityMultiplier,
            OverduePriorityMultiplier: entry.OverduePriorityMultiplier ?? TenantSchedulingRuleOverrides.Default.OverduePriorityMultiplier,
            MaxToleratedWarningViolations: entry.MaxToleratedWarningViolations ?? TenantSchedulingRuleOverrides.Default.MaxToleratedWarningViolations);
    }
}
