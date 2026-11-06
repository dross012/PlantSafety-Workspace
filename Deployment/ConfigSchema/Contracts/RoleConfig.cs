// ============================================================================
//  Project:        Deployment
//  Module:         ConfigSchema.Contracts
//  File:           RoleConfig.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. The deterministic, versioned role
//                  manifest. InheritsFromRoleIds is an explicit list the
//                  role author supplies -- RoleValidator (Boundaries/)
//                  walks it explicitly, never via reflection over other
//                  RoleConfig instances' properties.
// ============================================================================
using System.Text.Json.Serialization;

namespace Deployment.ConfigSchema.Contracts;

/// <summary>The deterministic, module-scoped permission vocabulary for deployment operations.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DeploymentPermission
{
    /// <summary>View deployment history, config snapshots, and tenant state.</summary>
    ViewDeployments,

    /// <summary>Approve a deployment for execution (a required gate for governance-controlled rollouts).</summary>
    ApproveDeployments,

    /// <summary>Execute a deployment (run DeploymentEngine steps).</summary>
    ExecuteDeployments,

    /// <summary>Create, update, suspend, or delete tenants.</summary>
    ManageTenants,

    /// <summary>Publish new config snapshots to the ConfigRegistry.</summary>
    ManageConfig,

    /// <summary>Rotate or manage secrets via the SecretsAdapter contract.</summary>
    ManageSecrets,
}

/// <summary>The deterministic, versioned role manifest.</summary>
public sealed record RoleConfig(
    DeploymentContractMetadata Metadata,
    string RoleId,
    string RoleName,
    IReadOnlyList<string> InheritsFromRoleIds,
    IReadOnlyList<DeploymentPermission> GrantedPermissions,
    bool RequiresMfa);
