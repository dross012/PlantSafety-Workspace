// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Configuration
//  Layer:          Application
//  File:           RollbackResultDto.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 7 (§7.2) outcome summary of a TriggerRollbackCommand execution.
//                  Additive-only evolution.
// ============================================================================
using System;

namespace PlantSafety.Application.Operations.Configuration.Models;

/// <summary>Summarizes the effect of a real, executed feature-flag rollback.</summary>
/// <param name="FeatureName">The feature flag that was rolled back.</param>
/// <param name="DatabaseKey">The database the global flag lives in.</param>
/// <param name="GlobalRolloutPercentageResetTo">The global flag's rollout percentage after the rollback (always 0).</param>
/// <param name="GlobalHardDisabled">Whether the global flag's IsEnabled was also forced to false.</param>
/// <param name="TenantOverridesReset">The number of tenant-level overrides whose rollout percentage was reset to 0.</param>
/// <param name="RegionOverridesReset">The number of region-level overrides whose rollout percentage was reset to 0.</param>
/// <param name="Reason">The operator/automation-supplied reason for the rollback.</param>
/// <param name="TriggeredUtc">When the rollback was executed.</param>
/// <param name="TriggeredBy">Who (or what automation) triggered the rollback.</param>
public sealed record RollbackResultDto(
    string FeatureName,
    string DatabaseKey,
    int GlobalRolloutPercentageResetTo,
    bool GlobalHardDisabled,
    int TenantOverridesReset,
    int RegionOverridesReset,
    string Reason,
    DateTime TriggeredUtc,
    string TriggeredBy);
