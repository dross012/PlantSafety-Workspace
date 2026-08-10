// ============================================================================
//  Project:        Deployment
//  Module:         ConfigSchema.Contracts
//  File:           DeploymentConfig.cs
//  Created:        2026-08-10
//  Description:    Phase 18 (Deployment Modernization) deliverable. The
//                  deterministic, versioned root deployment manifest.
//                  DeploymentContractMetadata is Deployment's own unified
//                  schema envelope, mirroring the exact pattern Shared.
//                  Scheduling/Shared.PredictiveSafety/Shared.SafetyGraph/
//                  Shared.Simulation already established for Phases 14-17
//                  (deterministic field ordering: version, tenant,
//                  correlation, then timestamp) -- no ContractBase exists in
//                  this program (Phase 12's own commit record), so this
//                  envelope is the "ContractMetadata" this phase's own
//                  requirements ask every contract to carry. Every enum
//                  carries [JsonConverter(typeof(JsonStringEnumConverter))]
//                  so System.Text.Json serializes deterministic, readable
//                  string values rather than integers -- the phase's own
//                  "explicit converters where needed" requirement.
// ============================================================================
using System.Text.Json.Serialization;

namespace Deployment.ConfigSchema.Contracts;

/// <summary>Deployment's module-scoped contract metadata: deterministic field ordering and versioning, tenant and correlation propagation.</summary>
public sealed record DeploymentContractMetadata(
    int Version,
    string TenantId,
    string CorrelationId,
    DateTime IssuedAtUtc)
{
    /// <summary>The current DeploymentContractMetadata schema version. Increment only on a breaking field-shape change.</summary>
    public const int CurrentVersion = 1;
}

/// <summary>The deterministic rollout strategy a deployment uses.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RolloutStrategyType
{
    /// <summary>Deploy to an idle environment, validate, then switch traffic atomically.</summary>
    BlueGreen,

    /// <summary>Deploy to an increasing traffic percentage in fixed, ascending stages.</summary>
    Canary,
}

/// <summary>The deterministic, versioned root deployment manifest.</summary>
public sealed record DeploymentConfig(
    DeploymentContractMetadata Metadata,
    string DeploymentId,
    string EnvironmentName,
    RolloutStrategyType Strategy,
    IReadOnlyList<string> ModuleIds,
    IReadOnlyList<int> CanaryStagePercentages,
    int HealthCheckTimeoutSeconds,
    int MaxRetries);
