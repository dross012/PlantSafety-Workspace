// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-20
// FILE DESCRIPTION: Client-side DTOs for the Operations.Configuration module. Mirror
//   PlantSafety.Application.Operations.Configuration.Models.* returned by
//   PlantSafety.Api.Operations.Configuration.Controllers.V1
//   .OperationsConfigurationV1Controller. Surfaces the real Phase 14 Governance
//   [System].[FeatureFlag] tables (144-feature-flags.sql), confirmed present ONLY in
//   PlantSafety.ArchiveDB and PlantSafety.IntegrationDB.
//   Extended 2026-08-03 (Phase 7 §6.1 Admin Portal / §7.1-7.2 Feature Flag System & Release
//   Pipeline) with the tenant/user/region override, resolution, release-channel, and rollback
//   DTOs backing OperationsConfigurationV1Controller's additive Phase 7 endpoints.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.OperationsConfiguration.Models;

/// <summary>A single DB-level ops feature flag row from a specific database's [System].[FeatureFlag] table.</summary>
public sealed class FeatureFlagDto
{
    /// <summary>Gets the logical database key the flag lives in (e.g. "ArchiveDB", "IntegrationDB").</summary>
    public string DatabaseKey { get; init; } = string.Empty;

    /// <summary>Gets the unique flag name (e.g. "RetentionPurge.Enabled").</summary>
    public string FlagName { get; init; } = string.Empty;

    /// <summary>Gets whether the flag is currently enabled.</summary>
    public bool IsEnabled { get; init; }

    /// <summary>Gets a human-readable description of what the flag controls.</summary>
    public string? Description { get; init; }

    /// <summary>Gets when the flag was last changed.</summary>
    public DateTime UpdatedUtc { get; init; }

    /// <summary>Gets who last changed the flag.</summary>
    public string UpdatedBy { get; init; } = string.Empty;

    /// <summary>Gets the optional canary-rollout percentage (0-100), or null when no percentage rollout is configured.</summary>
    public int? RolloutPercentage { get; init; }
}

/// <summary>A tenant-specific feature flag override (mirrors <c>TenantFeatureFlagOverrideDto</c>).</summary>
public sealed class TenantFeatureFlagOverrideDto
{
    /// <summary>Gets the tenant this override applies to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the feature flag name.</summary>
    public string FeatureName { get; init; } = string.Empty;

    /// <summary>Gets whether the feature is enabled for this tenant.</summary>
    public bool Enabled { get; init; }

    /// <summary>Gets the tenant-scoped canary-rollout percentage, if configured.</summary>
    public int? RolloutPercentage { get; init; }

    /// <summary>Gets when this override was last changed.</summary>
    public DateTime UpdatedUtc { get; init; }

    /// <summary>Gets who last changed this override.</summary>
    public string UpdatedBy { get; init; } = string.Empty;
}

/// <summary>A user-specific feature flag override (mirrors <c>UserFeatureFlagOverrideDto</c>).</summary>
public sealed class UserFeatureFlagOverrideDto
{
    /// <summary>Gets the user this override applies to.</summary>
    public Guid UserId { get; init; }

    /// <summary>Gets the feature flag name.</summary>
    public string FeatureName { get; init; } = string.Empty;

    /// <summary>Gets whether the feature is enabled for this user.</summary>
    public bool Enabled { get; init; }

    /// <summary>Gets when this override was last changed.</summary>
    public DateTime UpdatedUtc { get; init; }

    /// <summary>Gets who last changed this override.</summary>
    public string UpdatedBy { get; init; } = string.Empty;
}

/// <summary>A region-specific feature flag override (mirrors <c>RegionFeatureFlagOverrideDto</c>).</summary>
public sealed class RegionFeatureFlagOverrideDto
{
    /// <summary>Gets the region this override applies to.</summary>
    public string Region { get; init; } = string.Empty;

    /// <summary>Gets the feature flag name.</summary>
    public string FeatureName { get; init; } = string.Empty;

    /// <summary>Gets whether the feature is enabled in this region.</summary>
    public bool Enabled { get; init; }

    /// <summary>Gets the region-scoped canary-rollout percentage, if configured.</summary>
    public int? RolloutPercentage { get; init; }

    /// <summary>Gets when this override was last changed.</summary>
    public DateTime UpdatedUtc { get; init; }

    /// <summary>Gets who last changed this override.</summary>
    public string UpdatedBy { get; init; } = string.Empty;
}

/// <summary>The deterministic outcome of resolving a feature flag across all 4 precedence levels (mirrors <c>FeatureFlagResolutionResult</c>).</summary>
public sealed class FeatureFlagResolutionResultDto
{
    /// <summary>Gets the feature flag name that was resolved.</summary>
    public string FeatureName { get; init; } = string.Empty;

    /// <summary>Gets the final, resolved on/off decision.</summary>
    public bool IsEnabled { get; init; }

    /// <summary>Gets which precedence level decided the outcome (User/Tenant/Region/Global).</summary>
    public string DecidedBy { get; init; } = string.Empty;

    /// <summary>Gets the rollout percentage of the level that decided the outcome, if any was configured.</summary>
    public int? EffectiveRolloutPercentage { get; init; }

    /// <summary>Gets the tenant context the resolution was evaluated for, if any.</summary>
    public Guid? TenantId { get; init; }

    /// <summary>Gets the user context the resolution was evaluated for, if any.</summary>
    public Guid? UserId { get; init; }

    /// <summary>Gets the region context the resolution was evaluated for, if any.</summary>
    public string? Region { get; init; }
}

/// <summary>A tenant's assigned release channel (mirrors <c>TenantReleaseChannelDto</c>).</summary>
public sealed class TenantReleaseChannelDto
{
    /// <summary>Gets the tenant this channel assignment applies to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the assigned release channel name ("Stable", "Beta", or "Canary").</summary>
    public string Channel { get; init; } = string.Empty;

    /// <summary>Gets when the channel was last changed.</summary>
    public DateTime UpdatedUtc { get; init; }

    /// <summary>Gets who last changed the channel.</summary>
    public string UpdatedBy { get; init; } = string.Empty;
}

/// <summary>Summarizes the effect of a real, executed feature-flag rollback (mirrors <c>RollbackResultDto</c>).</summary>
public sealed class RollbackResultDto
{
    /// <summary>Gets the feature flag that was rolled back.</summary>
    public string FeatureName { get; init; } = string.Empty;

    /// <summary>Gets the database the global flag lives in.</summary>
    public string DatabaseKey { get; init; } = string.Empty;

    /// <summary>Gets the global flag's rollout percentage after the rollback (always 0).</summary>
    public int GlobalRolloutPercentageResetTo { get; init; }

    /// <summary>Gets whether the global flag's IsEnabled was also forced to false.</summary>
    public bool GlobalHardDisabled { get; init; }

    /// <summary>Gets the number of tenant-level overrides whose rollout percentage was reset to 0.</summary>
    public int TenantOverridesReset { get; init; }

    /// <summary>Gets the number of region-level overrides whose rollout percentage was reset to 0.</summary>
    public int RegionOverridesReset { get; init; }

    /// <summary>Gets the operator/automation-supplied reason for the rollback.</summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>Gets when the rollback was executed.</summary>
    public DateTime TriggeredUtc { get; init; }

    /// <summary>Gets who (or what automation) triggered the rollback.</summary>
    public string TriggeredBy { get; init; } = string.Empty;
}
