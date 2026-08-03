// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Configuration
//  Layer:          Application
//  File:           FeatureFlagSourceLevel.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 7 (§7.1) identifies which precedence level ultimately decided a
//                  resolved feature flag outcome, for auditability/observability. Ordered
//                  from highest to lowest precedence. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Operations.Configuration.Models;

/// <summary>
/// Identifies which level of the 4-level precedence chain (user > tenant > region > global)
/// decided a resolved feature flag's final outcome.
/// </summary>
public enum FeatureFlagSourceLevel
{
    /// <summary>No override at any level and no global flag row was found; the platform default (disabled) applied.</summary>
    Default = 0,

    /// <summary>The global, tenant-agnostic flag (from <c>[System].[FeatureFlag]</c>) decided the outcome.</summary>
    Global = 1,

    /// <summary>A <see cref="RegionFeatureFlagOverrideDto"/> decided the outcome.</summary>
    Region = 2,

    /// <summary>A <see cref="TenantFeatureFlagOverrideDto"/> decided the outcome.</summary>
    Tenant = 3,

    /// <summary>A <see cref="UserFeatureFlagOverrideDto"/> decided the outcome.</summary>
    User = 4
}
