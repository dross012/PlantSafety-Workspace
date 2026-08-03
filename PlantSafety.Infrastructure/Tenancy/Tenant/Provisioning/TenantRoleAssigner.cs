// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Infrastructure
//  File:           TenantRoleAssigner.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
//  SchemaVersion:  2026.1
//  Description:    ITenantRoleAssigner implementation. The authorization-ENFORCING role grant for
//                  the tenant's admin user is already written by CreateUserCommand's own Roles
//                  list (see TenantAdminUserProvisioner) -- this implementation additionally
//                  records the grant in the Tenancy schema's own
//                  [Tenancy].[TenantAdminRoleAssignment] audit table, using
//                  RoleInheritanceResolver to resolve and record the full inherited role set (not
//                  just the single highest-ranked role), so Tenancy's own provisioning-history
//                  view can show exactly what was granted without depending on EnterpriseFabric's
//                  internal user-role schema. Additive-only evolution.
// ============================================================================
using Shared.Authorization.Abstractions.Roles;
using PlantSafety.Application.Tenancy.Tenant.Abstractions;

namespace PlantSafety.Infrastructure.Tenancy.Tenant.Provisioning;

/// <summary>Records the Tenancy-scoped audit trail of a role grant made during tenant provisioning.</summary>
public sealed class TenantRoleAssigner(ISqlConnectionFactory connectionFactory, DapperCommandExecutor commandExecutor, ILogger<TenantRoleAssigner> logger)
    : ITenantRoleAssigner
{
    /// <inheritdoc/>
    public async Task AssignRoleAsync(Guid tenantId, Guid userId, EnterpriseRole role, Guid correlationId, CancellationToken cancellationToken)
    {
        var inheritedRoleNames = string.Join(",", RoleInheritanceResolver.GetInheritedRoleNames(role));

        using var conn = connectionFactory.CreateConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("UserId", userId)
            .AddString("GrantedRole", role.Name, 50)
            .AddString("InheritedRoleNames", inheritedRoleNames, 500)
            .AddGuid("CorrelationId", correlationId)
            .AddDateTime("GrantedAtUtc", DateTime.UtcNow)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Tenancy].[usp_TenantAdminRoleAssignment_Insert]", parameters, transaction: null, cancellationToken: cancellationToken);

        logger.LogInformation(
            "TenantProvisioning: Tenant {TenantId} admin user {UserId} granted role {Role} (inherits: {InheritedRoles}).",
            tenantId, userId, role.Name, inheritedRoleNames);
    }
}
