// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Configuration
//  Layer:          Application
//  File:           TenantReleaseChannel.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 7 (SaaS Productization Blueprint §7.2 Release Pipeline) per-tenant
//                  release channel. Feeds into FeatureFlagResolver's percentage-gated
//                  (canary) levels: a tenant on Beta or Canary auto-participates in an
//                  active partial rollout instead of being bucket-evaluated. See
//                  FeatureFlagResolver remarks for the exact semantics of each channel.
//                  Additive-only evolution: new channels may be appended; existing channel
//                  names/ordinals must never change.
// ============================================================================
namespace PlantSafety.Application.Operations.Configuration.Models;

/// <summary>
/// The release channel a tenant is subscribed to, controlling how that tenant participates
/// in percentage-gated (canary) feature rollouts.
/// </summary>
public enum TenantReleaseChannel
{
    /// <summary>
    /// Default channel. Percentage-gated rollouts are evaluated normally via the
    /// deterministic SHA-256 bucket (<see cref="Rollout.ITenantRolloutPolicy"/>) -- the
    /// tenant is included only if its bucket falls under the configured percentage.
    /// </summary>
    Stable = 0,

    /// <summary>
    /// Opt-in beta channel. Any feature with an active partial rollout (a non-null
    /// <c>RolloutPercentage</c> greater than 0) is treated as enabled for this tenant
    /// regardless of its bucket -- beta tenants always see features that have started
    /// rolling out to anyone.
    /// </summary>
    Beta = 1,

    /// <summary>
    /// Earliest-access channel. Any feature with a percentage-gated configuration at all
    /// (a non-null <c>RolloutPercentage</c>, including 0 -- i.e. configured but not yet
    /// started for the general population) is treated as enabled for this tenant, so
    /// canary tenants can validate a feature before its rollout wave begins for anyone
    /// else.
    /// </summary>
    Canary = 2
}
