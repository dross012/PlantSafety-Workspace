// ============================================================================
//  Project:        Deployment
//  Module:         ConfigSchema.Contracts
//  File:           ModuleConfig.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. The deterministic, versioned module
//                  boundary manifest, plus the feature-flag contract this
//                  phase's Boundaries scope asks for. DependsOnModuleIds is
//                  an explicit list -- DeploymentEngine/DriftReconciler walk
//                  it explicitly, never via reflection.
// ============================================================================
using System.Text.Json.Serialization;

namespace Deployment.ConfigSchema.Contracts;

/// <summary>The deterministic lifecycle state a module is deployed in.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ModuleLifecycleState
{
    /// <summary>The module is fully enabled for the owning tenant.</summary>
    Enabled,

    /// <summary>The module is disabled -- present in configuration but not deployed.</summary>
    Disabled,

    /// <summary>The module is enabled for a subset of callers under a feature flag, not generally available.</summary>
    Preview,
}

/// <summary>A single, deterministic feature flag.</summary>
public sealed record FeatureFlagConfig(
    string FlagId,
    bool IsEnabled,
    string Description);

/// <summary>The deterministic, versioned module boundary manifest.</summary>
public sealed record ModuleConfig(
    DeploymentContractMetadata Metadata,
    string ModuleId,
    string ModuleName,
    ModuleLifecycleState State,
    IReadOnlyList<FeatureFlagConfig> FeatureFlags,
    IReadOnlyList<string> DependsOnModuleIds);
