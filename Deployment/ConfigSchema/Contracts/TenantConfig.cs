// ============================================================================
//  Project:        Deployment
//  Module:         ConfigSchema.Contracts
//  File:           TenantConfig.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. The deterministic, versioned tenant
//                  manifest: topology (Shared vs Isolated), lifecycle
//                  status, resource quota, and the explicit module/role
//                  assignments that scope this tenant's deployment
//                  boundary. Resource quotas are explicit integer/long
//                  fields, never computed via reflection over other
//                  contracts, per this phase's "no reflection" constraint.
// ============================================================================
namespace Deployment.ConfigSchema.Contracts;

/// <summary>The deterministic tenant isolation topology.</summary>
[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum TenantTopology
{
    /// <summary>The tenant shares compute/network with other tenants, isolated only by namespace scoping.</summary>
    Shared,

    /// <summary>The tenant has dedicated compute, network, and storage scoping.</summary>
    Isolated,
}

/// <summary>The deterministic tenant lifecycle status.</summary>
[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum TenantStatus
{
    /// <summary>The tenant is active and may receive deployments.</summary>
    Active,

    /// <summary>The tenant is suspended -- no new deployments may target it, existing deployments are unaffected.</summary>
    Suspended,

    /// <summary>The tenant has been deleted -- terminal state.</summary>
    Deleted,
}

/// <summary>The deterministic resource quota a tenant is bound to.</summary>
public sealed record TenantResourceQuota(
    int MaxMachineGroups,
    int MaxConcurrentDeployments,
    long MaxStorageBytes);

/// <summary>The deterministic, versioned tenant manifest.</summary>
public sealed record TenantConfig(
    DeploymentContractMetadata Metadata,
    string TenantId,
    string TenantName,
    TenantTopology Topology,
    TenantStatus Status,
    TenantResourceQuota ResourceQuota,
    IReadOnlyList<string> AssignedModuleIds,
    IReadOnlyList<string> AssignedRoleIds);
