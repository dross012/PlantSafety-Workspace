// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Permissions
//  Layer:          Abstractions
//  File:           ModulePermissionVerb.cs
//  Created:        2026-08-02
//  Author:         Enterprise Integration & Ecosystem Expansion Agent
//  Description:    Canonical permission verb vocabulary, mirroring the verb set
//                  already inferred by
//                  PlantSafety.Api.Extensions.AuthorizationPolicyExtensions'
//                  TierForFieldName mapping (Read/Write/Approve/Admin) so
//                  Permissions.IPermissionResolver and that reflection-based
//                  policy discovery stay in lockstep.
// ============================================================================

namespace Shared.Authorization.Abstractions.Permissions;

/// <summary>
/// Canonical permission verbs. Each verb corresponds 1:1 to an enterprise tier
/// (see <see cref="Roles.EnterpriseRole"/>) and to the tier inference already
/// performed by <c>AuthorizationPolicyExtensions.TierForFieldName</c>.
/// </summary>
public enum ModulePermissionVerb
{
    /// <summary>Read-only access. Granted by <see cref="Roles.EnterpriseRole.Viewer"/> and above.</summary>
    Read = 0,

    /// <summary>Create/update access. Granted by <see cref="Roles.EnterpriseRole.Editor"/> and above.</summary>
    Write = 1,

    /// <summary>Approve/close/escalate/dispatch/resolve access. Granted by <see cref="Roles.EnterpriseRole.Approver"/> and above.</summary>
    Approve = 2,

    /// <summary>Destructive/administrative access. Granted only by <see cref="Roles.EnterpriseRole.PlatformAdmin"/>.</summary>
    Admin = 3
}
